using AmeisenBotRevamped.ActionExecutors.Enums;
using AmeisenBotRevamped.Logging;
using AmeisenBotRevamped.Logging.Enums;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using AmeisenBotRevamped.OffsetLists;
using AmeisenBotRevamped.Utils;
using TrashMemCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AmeisenBotRevamped.ActionExecutors
{
    public class MemoryWowActionExecutor : IWowActionExecutor
    {
        #region Properties
        public bool IsWoWHooked => TrashMem.ReadChar(EndsceneAddress) == 0xE9;

        public bool IsWorldLoaded { get; set; }
        public int ProcessId => TrashMem.Process.Id;
        #endregion

        #region Internal Properties
        // You may not need them but who knows
        public const uint ENDSCENE_HOOK_OFFSET = 0x2;

        public IOffsetList OffsetList { get; private set; }
        public TrashMem TrashMem { get; private set; }

        public byte[] OriginalEndsceneBytes { get; private set; }

        public uint EndsceneAddress { get; private set; }
        public uint EndsceneReturnAddress { get; private set; }
        public uint CodeToExecuteAddress { get; private set; }
        public uint ReturnValueAddress { get; private set; }
        public uint CodecaveForCheck { get; private set; }
        public uint CodecaveForExecution { get; private set; }
        public bool IsInjectionUsed { get; private set; }

        public Dictionary<(int, int), UnitReaction> ReactionCache { get; private set; }
        #endregion

        public MemoryWowActionExecutor(TrashMem trashMem, IOffsetList offsetList)
        {
            ReactionCache = new Dictionary<(int, int), UnitReaction>();
            OriginalEndsceneBytes = offsetList.EndSceneBytes;
            TrashMem = trashMem;
            OffsetList = offsetList;

            EndsceneAddress = GetEndScene();
            SetupEndsceneHook();
        }

        public void AttackTarget()
            => LuaDoString($"AttackTarget();");

        public void CastSpell(int spellId)
            => LuaDoString($"CastSpell({spellId});");

        public void KickNpcsOutOfMammoth()
            => LuaDoString("for i = 1, 2 do EjectPassengerFromSeat(i) end");

        public void LootEveryThing()
            => LuaDoString("abLootCount=GetNumLootItems();for i = abLootCount,1,-1 do LootSlot(i); ConfirmLootSlot(i); end");

        public void ReleaseSpirit() => LuaDoString("RepopMe();");

        public void RepairAllItems() => LuaDoString("RepairAllItems();");

        public void RetrieveCorpse() => LuaDoString("RetrieveCorpse();");

        public double GetSpellCooldown(string spellName)
        {
            LuaDoString($"start,duration,enabled = GetSpellCooldown(\"{spellName}\");cdLeft = (start + duration - GetTime()) * 1000;");
            if (double.TryParse(GetLocalizedText("cdLeft"), out double value))
            {
                return value;
            }
            return -1;
        }

        public void CastSpell(string spellName, bool castOnSelf = false)
        {
            if (castOnSelf)
            {
                LuaDoString($"CastSpellByName(\"{spellName}\", true);");
            }
            else
            {
                LuaDoString($"CastSpellByName(\"{spellName}\");");
            }
        }

        public void FaceUnit(WowPlayer player, WowPosition positionToFace)
        {
            float angle = BotMath.GetFacingAngle(player.Position, positionToFace);
            TrashMem.Write(player.BaseAddress + OffsetList.OffsetPlayerRotation, angle);
            SendKey(new IntPtr(0x41), 0, 0); // the "S" key to go a bit backwards TODO: find better method 0x53
        }

        public void SendKey(IntPtr vKey, int minDelay = 20, int maxDelay = 40)
        {
            const uint KEYDOWN = 0x100;
            const uint KEYUP = 0x101;

            IntPtr windowHandle = TrashMem.Process.MainWindowHandle;

            // 0x20 = Spacebar (VK_SPACE)
            SafeNativeMethods.SendMessage(windowHandle, KEYDOWN, vKey, new IntPtr(0));
            Thread.Sleep(new Random().Next(minDelay, maxDelay)); // make it look more human-like :^)
            SafeNativeMethods.SendMessage(windowHandle, KEYUP, vKey, new IntPtr(0));
        }

        public int GetCorpseCooldown()
        {
            LuaDoString("corpseDelay = GetCorpseRecoveryDelay();");
            if (int.TryParse(GetLocalizedText("corpseDelay"), out int value))
            {
                return value;
            }
            return 0;
        }

        public void InteractWithGuid(ulong guid, ClickToMoveType clickToMoveType = ClickToMoveType.Interact)
        {
            TrashMem.Write(OffsetList.StaticClickToMoveGuid, guid);
            TrashMem.Write(OffsetList.StaticClickToMoveAction, (int)clickToMoveType);
        }

        public void MoveToPosition(WowPosition targetPosition, ClickToMoveType clickToMoveType = ClickToMoveType.Move, float distance = 1.5f)
        {
            TrashMem.Write(OffsetList.StaticClickToMoveX, targetPosition.x);
            TrashMem.Write(OffsetList.StaticClickToMoveY, targetPosition.y);
            TrashMem.Write(OffsetList.StaticClickToMoveZ, targetPosition.z);
            TrashMem.Write(OffsetList.StaticClickToMoveDistance, distance);
            TrashMem.Write(OffsetList.StaticClickToMoveAction, (int)clickToMoveType);
        }

        public void TargetGuid(ulong guid)
        {
            byte[] guidBytes = BitConverter.GetBytes(guid);
            string[] asm = new string[]
            {
                $"PUSH {BitConverter.ToUInt32(guidBytes, 4)}",
                $"PUSH {BitConverter.ToUInt32(guidBytes, 0)}",
                $"CALL {OffsetList.FunctionSetTarget}",
                "ADD ESP, 0x8",
                "RETN"
            };
            InjectAndExecute(asm, false);
        }

        public void LuaDoString(string command)
        {
            AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tExecuting Lua \"{command}\"", LogLevel.Verbose);
            if (command.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(command);
                uint argCC = TrashMem.AllocateMemory(bytes.Length + 1);
                TrashMem.WriteBytes(argCC, bytes);

                if (argCC == 0)
                    return;

                string[] asm = new string[]
                {
                    $"MOV EAX, {(argCC)}",
                    "PUSH 0",
                    "PUSH EAX",
                    "PUSH EAX",
                    $"CALL {OffsetList.FunctionLuaDoString}",
                    "ADD ESP, 0xC",
                    "RETN",
                };

                InjectAndExecute(asm, false);
                TrashMem.FreeMemory(argCC);
            }
        }

        public string GetLocalizedText(string variable)
        {
            AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tReading Lua variable \"{variable}\"", LogLevel.Verbose);
            if (variable.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(variable);
                uint argCC = TrashMem.AllocateMemory(bytes.Length + 1);
                TrashMem.WriteBytes(argCC, bytes);

                string[] asmLocalText = new string[]
                {
                    $"CALL {OffsetList.FunctionGetActivePlayerObject}",
                    "MOV ECX, EAX",
                    "PUSH -1",
                    $"PUSH {(argCC)}",
                    $"CALL {OffsetList.FunctionGetLocalizedText}",
                    "RETN",
                };

                string result = Encoding.UTF8.GetString(InjectAndExecute(asmLocalText, true));
                TrashMem.FreeMemory(argCC);
                return result;
            }
            return "";
        }

        private void SetupEndsceneHook()
        {
            // first thing thats 5 bytes big is here
            // we are going to replace this 5 bytes with
            // our JMP instruction (JMP (1 byte) + Address (4 byte))
            EndsceneAddress += ENDSCENE_HOOK_OFFSET;

            // if WoW is already hooked, unhook it
            if (IsWoWHooked) { DisposeHook(); }
            else { OriginalEndsceneBytes = TrashMem.ReadChars(EndsceneAddress, 5); }

            // if WoW is now/was unhooked, hook it
            if (!IsWoWHooked)
            {
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tHooking EndScene at \"{EndsceneAddress.ToString("X")}\"", LogLevel.Verbose);

                // the address that we will return to after 
                // the jump wer'e going to inject
                EndsceneReturnAddress = EndsceneAddress + 0x5;

                // read our original EndScene
                //OriginalEndsceneBytes = TrashMem.ReadChars(EndsceneAddress, 5);

                // integer to check if there is code waiting to be executed
                CodeToExecuteAddress = TrashMem.AllocateMemory(4);
                TrashMem.Write(CodeToExecuteAddress, 0);

                // integer to save the address of the return value
                ReturnValueAddress = TrashMem.AllocateMemory(4);
                TrashMem.Write(ReturnValueAddress, 0);

                // codecave to check if we need to execute something
                CodecaveForCheck = TrashMem.AllocateMemory(1024);
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tCCCheck is at \"{CodecaveForCheck.ToString("X")}\"", LogLevel.Verbose);
                // codecave for the code we wa't to execute
                CodecaveForExecution = TrashMem.AllocateMemory(8192);
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tCCExecution is at \"{CodecaveForExecution.ToString("X")}\"", LogLevel.Verbose);

                TrashMem.Asm.Clear();
                // save registers
                TrashMem.Asm.AddLine("PUSHFD");
                TrashMem.Asm.AddLine("PUSHAD");

                // check for code to be executed
                TrashMem.Asm.AddLine($"MOV EBX, [{CodeToExecuteAddress}]");
                TrashMem.Asm.AddLine("TEST EBX, 1");
                TrashMem.Asm.AddLine("JE @out");

                // execute our stuff and get return address
                TrashMem.Asm.AddLine($"MOV EDX, {CodecaveForExecution}");
                TrashMem.Asm.AddLine("CALL EDX");
                TrashMem.Asm.AddLine($"MOV [{ReturnValueAddress}], EAX");

                // finish up our execution
                TrashMem.Asm.AddLine("@out:");
                TrashMem.Asm.AddLine("MOV EDX, 0");
                TrashMem.Asm.AddLine($"MOV [{CodeToExecuteAddress}], EDX");

                // restore registers
                TrashMem.Asm.AddLine("POPAD");
                TrashMem.Asm.AddLine("POPFD");

                // needed to determine the position where the original
                // asm is going to be placed
                int asmLenght = TrashMem.Asm.Assemble().Length;

                // inject the instructions into our codecave
                byte[] asmBytes = TrashMem.Asm.Assemble();
                TrashMem.WriteBytes(CodecaveForCheck, asmBytes);
                // ---------------------------------------------------
                // End of the code that checks if there is asm to be
                // executed on our hook
                // ---------------------------------------------------

                // Prepare to replace the instructions inside WoW
                TrashMem.Asm.Clear();

                // do the original EndScene stuff after we restored the registers
                // and insert it after our code
                TrashMem.WriteBytes(CodecaveForCheck + (uint)asmLenght, OriginalEndsceneBytes);

                // return to original function after we're done with our stuff
                TrashMem.Asm.AddLine($"JMP {EndsceneReturnAddress}");
                byte[] asmBytes2 = TrashMem.Asm.Assemble();
                TrashMem.WriteBytes(CodecaveForCheck + (uint)asmLenght + 5, asmBytes2);
                TrashMem.Asm.Clear();
                // ---------------------------------------------------
                // End of doing the original stuff and returning to
                // the original instruction
                // ---------------------------------------------------

                // modify original EndScene instructions to start the hook
                TrashMem.Asm.AddLine($"JMP {CodecaveForCheck}");
                byte[] asmBytes3 = TrashMem.Asm.Assemble();
                TrashMem.WriteBytes(EndsceneAddress, asmBytes3);
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tInjected Hook [IsWoWHooked = {IsWoWHooked}]", LogLevel.Verbose);
                // we should've hooked WoW now
            }
        }

        private void DisposeHook()
        {
            try
            {
                if (IsWoWHooked)
                {
                    AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tDisposing Hook", LogLevel.Verbose);
                    TrashMem.WriteBytes(EndsceneAddress, OriginalEndsceneBytes);

                    TrashMem.FreeMemory(CodecaveForCheck);
                    TrashMem.FreeMemory(CodecaveForExecution);
                    TrashMem.FreeMemory(CodeToExecuteAddress);
                    TrashMem.FreeMemory(ReturnValueAddress);
                }
            }
            catch { }
        }

        private uint GetEndScene()
        {
            uint pDevice = TrashMem.ReadUInt32(OffsetList.StaticEndSceneDevice);
            uint pEnd = TrashMem.ReadUInt32(pDevice + OffsetList.EndSceneOffsetDevice);
            uint pScene = TrashMem.ReadUInt32(pEnd);
            return TrashMem.ReadUInt32(pScene + OffsetList.EndSceneOffset);
        }

        public byte[] InjectAndExecute(string[] asm, bool readReturnBytes)
        {
            AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tInjecting ASM into Hook [asm = {JsonConvert.SerializeObject(asm)}, readReturnBytes = {readReturnBytes}]", LogLevel.Verbose);
            List<byte> returnBytes = new List<byte>();

            if (!IsWorldLoaded)
            {
                return returnBytes.ToArray();
            }

            try
            {
                // wait for the code to be executed
                while (IsInjectionUsed) { Thread.Sleep(1); }

                IsInjectionUsed = true;
                // preparing to inject the given ASM
                TrashMem.Asm.Clear();
                // add all lines
                foreach (string s in asm)
                {
                    TrashMem.Asm.AddLine(s);
                }

                // now there is code to be executed
                TrashMem.Write(CodeToExecuteAddress, 1);
                // inject it
                byte[] asmBytes = TrashMem.Asm.Assemble();
                TrashMem.WriteBytes(CodecaveForExecution, asmBytes);

                // wait for the code to be executed
                while (TrashMem.ReadInt32(CodeToExecuteAddress) > 0) { Thread.Sleep(1); }

                // if we want to read the return value do it otherwise we're done
                if (readReturnBytes)
                {
                    byte buffer = new byte();
                    try
                    {
                        uint dwAddress = TrashMem.ReadUnmanaged<uint>(ReturnValueAddress);

                        // read all parameter-bytes until we the buffer is 0
                        buffer = TrashMem.ReadChar(dwAddress);
                        while (buffer != 0)
                        {
                            returnBytes.Add(buffer);
                            dwAddress++;
                            buffer = TrashMem.ReadChar(dwAddress);
                        }
                    }
                    catch (Exception e)
                    {
                        AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tCrash at reading the return bytes: \n{e.ToString()}", LogLevel.Error);
                    }
                }
            }
            catch (Exception e)
            {
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tCrash at injecting: \n{e.ToString()}", LogLevel.Error);
                // now there is no more code to be executed
                TrashMem.ReadUnmanaged<uint>(CodeToExecuteAddress, 0);
            }
            IsInjectionUsed = false;

            return returnBytes.ToArray();
        }

        public void AntiAfk() => TrashMem.Write(OffsetList.StaticTickCount, Environment.TickCount);

        public void Jump() => SendKey(new IntPtr(0x20));

        public void Stop()
        {
            DisposeHook();
        }

        public void AcceptPartyInvite()
        {
            LuaDoString("AcceptGroup();");
            SendChatMessage("/click StaticPopup1Button1");
        }

        public void AcceptResurrect()
        {
            LuaDoString("AcceptResurrect();");
            SendChatMessage("/click StaticPopup1Button1");
        }

        public void AcceptSummon()
        {
            LuaDoString("ConfirmSummon();");
            SendChatMessage("/click StaticPopup1Button1");
        }

        public void SendChatMessage(string command)
            => LuaDoString($"DEFAULT_CHAT_FRAME.editBox:SetText(\"{command}\") ChatEdit_SendText(DEFAULT_CHAT_FRAME.editBox, 0)");

        public UnitReaction GetUnitReaction(WowUnit wowUnitA, WowUnit wowUnitB)
        {
            AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tGetting relation of: {wowUnitA.Name} to {wowUnitB.Name}", LogLevel.Verbose);

            if (ReactionCache.ContainsKey((wowUnitA.FactionTemplate, wowUnitB.FactionTemplate)))
            {
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tCached relation of: {wowUnitA.Name} to {wowUnitB.Name} is {ReactionCache[(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate)].ToString()}", LogLevel.Verbose);
                return ReactionCache[(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate)];
            }

            // integer to save the reaction
            uint reactionValue = TrashMem.AllocateMemory(4);
            TrashMem.Write(reactionValue, 0);

            string[] asm = new string[]
            {
                $"PUSH " + wowUnitA.BaseAddress,
                $"MOV ECX, " + wowUnitB.BaseAddress,
                $"CALL " + OffsetList.FunctionGetUnitReaction,
                $"MOV [{reactionValue}], EAX",
                "RETN",
            };

            byte[] reactionBytes = InjectAndExecute(asm, true);

            /*if (reactionBytes.Length == 0)
            {
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tRelation of: {wowUnitA.Name} to {wowUnitB.Name} is UnitReaction.Unknown", LogLevel.Verbose);
                return UnitReaction.Unknown;
            }*/
            UnitReaction reaction;

            try
            {
                reaction = (UnitReaction)TrashMem.ReadInt32(reactionValue);

                //UnitReaction reaction = (UnitReaction)BitConverter.ToInt32(reactionBytes, 0);

                ReactionCache.Add((wowUnitA.FactionTemplate, wowUnitB.FactionTemplate), reaction);
            }
            catch
            {
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tFailed to read relation of: {wowUnitA.Name} to {wowUnitB.Name} is Unknown", LogLevel.Verbose);
                return UnitReaction.Unknown;
            }
            finally
            {
                TrashMem.FreeMemory(reactionValue);
            }

            AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tRelation of: {wowUnitA.Name} to {wowUnitB.Name} is {reaction.ToString()}", LogLevel.Verbose);
            return reaction;
        }

        public void RightClickUnit(WowUnit wowUnit)
        {
            AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tDoing RightClick: {wowUnit.Name}", LogLevel.Verbose);
            string[] asm = new string[]
            {
                $"MOV ECX, {wowUnit.BaseAddress}",
                "MOV EAX, DWORD[ECX]",
                "MOV EAX, DWORD[EAX + 88H]",
                "CALL EAX",
                "RETN",
            };
            InjectAndExecute(asm, false);
        }

        public List<string> GetAuras(string luaUnitName)
        {
            List<string> result = new List<string>(GetBuffs(luaUnitName));
            result.AddRange(GetDebuffs(luaUnitName));
            return result;
        }

        public List<string> GetBuffs(string luaUnitName)
        {
            List<string> resultLowered = new List<string>();
            StringBuilder cmdBuffs = new StringBuilder();
            cmdBuffs.Append("local buffs, i = { }, 1;");
            cmdBuffs.Append($"local buff = UnitBuff(\"{luaUnitName}\", i);");
            cmdBuffs.Append("while buff do\n");
            cmdBuffs.Append("buffs[#buffs + 1] = buff;");
            cmdBuffs.Append("i = i + 1;");
            cmdBuffs.Append($"buff = UnitBuff(\"{luaUnitName}\", i);");
            cmdBuffs.Append("end;");
            cmdBuffs.Append("if #buffs < 1 then\n");
            cmdBuffs.Append("buffs = \"\";");
            cmdBuffs.Append("else\n");
            cmdBuffs.Append("activeUnitBuffs = table.concat(buffs, \", \");");
            cmdBuffs.Append("end;");

            LuaDoString(cmdBuffs.ToString());
            string[] buffs = GetLocalizedText("activeUnitBuffs").Split(',');

            foreach (string s in buffs)
            {
                resultLowered.Add(s.Trim().ToLower());
            }

            return resultLowered;
        }

        public List<string> GetDebuffs(string luaUnitName)
        {
            List<string> resultLowered = new List<string>();
            StringBuilder cmdDebuffs = new StringBuilder();
            cmdDebuffs.Append("local buffs, i = { }, 1;");
            cmdDebuffs.Append($"local buff = UnitDebuff(\"{luaUnitName}\", i);");
            cmdDebuffs.Append("while buff do\n");
            cmdDebuffs.Append("buffs[#buffs + 1] = buff;");
            cmdDebuffs.Append("i = i + 1;");
            cmdDebuffs.Append($"buff = UnitDebuff(\"{luaUnitName}\", i);");
            cmdDebuffs.Append("end;");
            cmdDebuffs.Append("if #buffs < 1 then\n");
            cmdDebuffs.Append("buffs = \"\";");
            cmdDebuffs.Append("else\n");
            cmdDebuffs.Append("activeUnitDebuffs = table.concat(buffs, \", \");");
            cmdDebuffs.Append("end;");

            LuaDoString(cmdDebuffs.ToString());
            string[] debuffs = GetLocalizedText("activeUnitDebuffs").Split(',');

            foreach (string s in debuffs)
            {
                resultLowered.Add(s.Trim().ToLower());
            }

            return resultLowered;
        }

        public void CofirmBop()
        {
            LuaDoString("ConfirmBindOnUse();");
            SendChatMessage("/click StaticPopup1Button1");
        }

        public void CofirmReadyCheck(bool isReady)
        {
            LuaDoString($"ConfirmReadyCheck({isReady});");
        }
    }
}
