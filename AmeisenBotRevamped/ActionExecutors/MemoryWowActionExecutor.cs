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
using TrashMemCore.Objects;
using Fasm;
using System.Collections.Specialized;

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
        public MemoryAllocation CodeToExecuteAddress { get; private set; }
        public MemoryAllocation ReturnValueAddress { get; private set; }
        public MemoryAllocation CodecaveForCheck { get; private set; }
        public MemoryAllocation CodecaveForExecution { get; private set; }
        public bool IsInjectionUsed { get; private set; }
        #endregion

        public MemoryWowActionExecutor(TrashMem trashMem, IOffsetList offsetList)
        {
            OriginalEndsceneBytes = offsetList.EndSceneBytes;
            TrashMem = trashMem;
            OffsetList = offsetList;

            EndsceneAddress = GetEndScene();
            SetupEndsceneHook();
        }

        public void AttackUnit(WowUnit unit)
        {
            TrashMem.Write(OffsetList.StaticClickToMoveGuid, unit.Guid);
            MoveToPosition(unit.Position, ClickToMoveType.AttackGuid);
        }

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
                MemoryAllocation memAlloc = TrashMem.AllocateMemory(bytes.Length + 1);
                if (memAlloc == null) return;

                TrashMem.WriteBytes(memAlloc.Address, bytes);

                if (memAlloc.Address == 0)
                    return;

                string[] asm = new string[]
                {
                    $"MOV EAX, {memAlloc.Address}",
                    "PUSH 0",
                    "PUSH EAX",
                    "PUSH EAX",
                    $"CALL {OffsetList.FunctionLuaDoString}",
                    "ADD ESP, 0xC",
                    "RETN",
                };

                InjectAndExecute(asm, false);
                TrashMem.FreeMemory(memAlloc);
            }
        }

        public string GetLocalizedText(string variable)
        {
            AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tReading Lua variable \"{variable}\"", LogLevel.Verbose);
            if (variable.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(variable);
                MemoryAllocation memAlloc = TrashMem.AllocateMemory(bytes.Length + 1);
                if (memAlloc == null) return "";

                TrashMem.WriteBytes(memAlloc.Address, bytes);

                string[] asmLocalText = new string[]
                {
                    $"CALL {OffsetList.FunctionGetActivePlayerObject}",
                    "MOV ECX, EAX",
                    "PUSH -1",
                    $"PUSH {memAlloc.Address}",
                    $"CALL {OffsetList.FunctionGetLocalizedText}",
                    "RETN",
                };

                string result = Encoding.UTF8.GetString(InjectAndExecute(asmLocalText, true));
                TrashMem.FreeMemory(memAlloc);
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
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tHooking EndScene at \"0x{EndsceneAddress.ToString("X")}\"", LogLevel.Verbose);

                // the address that we will return to after 
                // the jump wer'e going to inject
                EndsceneReturnAddress = EndsceneAddress + 0x5;

                // read our original EndScene
                //OriginalEndsceneBytes = TrashMem.ReadChars(EndsceneAddress, 5);

                // integer to check if there is code waiting to be executed
                CodeToExecuteAddress = TrashMem.AllocateMemory(4);
                TrashMem.Write(CodeToExecuteAddress.Address, 0);

                // integer to save the address of the return value
                ReturnValueAddress = TrashMem.AllocateMemory(4);
                TrashMem.Write(ReturnValueAddress.Address, 0);

                // codecave to check if we need to execute something
                CodecaveForCheck = TrashMem.AllocateMemory(128);
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tCCCheck is at \"0x{CodecaveForCheck.Address.ToString("X")}\"", LogLevel.Verbose);
                // codecave for the code we wa't to execute
                CodecaveForExecution = TrashMem.AllocateMemory(4096);
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tCCExecution is at \"0x{CodecaveForExecution.Address.ToString("X")}\"", LogLevel.Verbose);

                TrashMem.Asm.Clear();
                // save registers
                TrashMem.Asm.AddLine("PUSHFD");
                TrashMem.Asm.AddLine("PUSHAD");

                // check for code to be executed
                TrashMem.Asm.AddLine($"MOV EBX, [{CodeToExecuteAddress.Address}]");
                TrashMem.Asm.AddLine("TEST EBX, 1");
                TrashMem.Asm.AddLine("JE @out");

                // execute our stuff and get return address
                TrashMem.Asm.AddLine($"MOV EDX, {CodecaveForExecution.Address}");
                TrashMem.Asm.AddLine("CALL EDX");
                TrashMem.Asm.AddLine($"MOV [{ReturnValueAddress.Address}], EAX");

                // finish up our execution
                TrashMem.Asm.AddLine("@out:");
                TrashMem.Asm.AddLine("MOV EDX, 0");
                TrashMem.Asm.AddLine($"MOV [{CodeToExecuteAddress.Address}], EDX");

                // restore registers
                TrashMem.Asm.AddLine("POPAD");
                TrashMem.Asm.AddLine("POPFD");

                byte[] asmBytes = TrashMem.Asm.Assemble();

                // needed to determine the position where the original
                // asm is going to be placed
                int asmLenght = asmBytes.Length;

                // inject the instructions into our codecave
                TrashMem.Asm.Inject(CodecaveForCheck.Address);
                // ---------------------------------------------------
                // End of the code that checks if there is asm to be
                // executed on our hook
                // ---------------------------------------------------

                // Prepare to replace the instructions inside WoW
                TrashMem.Asm.Clear();

                // do the original EndScene stuff after we restored the registers
                // and insert it after our code
                TrashMem.WriteBytes(CodecaveForCheck.Address + (uint)asmLenght, OriginalEndsceneBytes);

                // return to original function after we're done with our stuff
                TrashMem.Asm.AddLine($"JMP {EndsceneReturnAddress}");
                //byte[] asmBytes2 = TrashMem.Asm.Assemble();
                TrashMem.Asm.Inject(CodecaveForCheck.Address + (uint)asmLenght + 5);
                TrashMem.Asm.Clear();
                // ---------------------------------------------------
                // End of doing the original stuff and returning to
                // the original instruction
                // ---------------------------------------------------

                // modify original EndScene instructions to start the hook
                TrashMem.Asm.AddLine($"JMP {CodecaveForCheck.Address}");
                //byte[] asmBytes3 = TrashMem.Asm.Assemble();
                TrashMem.Asm.Inject(EndsceneAddress);
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

                    if (CodecaveForCheck != null)
                        TrashMem.FreeMemory(CodecaveForCheck);
                    if (CodecaveForExecution != null)
                        TrashMem.FreeMemory(CodecaveForExecution);
                    if (CodeToExecuteAddress != null)
                        TrashMem.FreeMemory(CodeToExecuteAddress);
                    if (ReturnValueAddress != null)
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
                TrashMem.Write(CodeToExecuteAddress.Address, 1);
                // inject it
                //byte[] asmBytes = TrashMem.Asm.Assemble();
                TrashMem.Asm.Inject(CodecaveForExecution.Address);

                // wait for the code to be executed
                while (TrashMem.ReadInt32(CodeToExecuteAddress.Address) > 0) { Thread.Sleep(1); }

                // if we want to read the return value do it otherwise we're done
                if (readReturnBytes)
                {
                    byte buffer = new byte();
                    try
                    {
                        uint dwAddress = TrashMem.ReadUnmanaged<uint>(ReturnValueAddress.Address);

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
                IsInjectionUsed = false;
            }
            catch (Exception e)
            {
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tCrash at injecting: \n{e.ToString()}", LogLevel.Error);
                // now there is no more code to be executed
                TrashMem.ReadUnmanaged<uint>(CodeToExecuteAddress.Address, 0);
                IsInjectionUsed = false;
            }

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
            UnitReaction reaction = UnitReaction.Unknown;

            if (SharedCacheManager.Instance.ReactionCache.ContainsKey((wowUnitA.FactionTemplate, wowUnitB.FactionTemplate)))
            {
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tCached relation of: {wowUnitA.Name} to {wowUnitB.Name} is {SharedCacheManager.Instance.ReactionCache[(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate)].ToString()}", LogLevel.Verbose);
                return SharedCacheManager.Instance.ReactionCache[(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate)];
            }

            // integer to save the reaction
            MemoryAllocation memAlloc = TrashMem.AllocateMemory(4);
            TrashMem.Write(memAlloc.Address, 0);

            string[] asm = new string[]
            {
                $"PUSH " + wowUnitA.BaseAddress,
                $"MOV ECX, " + wowUnitB.BaseAddress,
                $"CALL " + OffsetList.FunctionGetUnitReaction,
                $"MOV [{memAlloc.Address}], EAX",
                "RETN",
            };

            /*if (reactionBytes.Length == 0)
            {
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tRelation of: {wowUnitA.Name} to {wowUnitB.Name} is UnitReaction.Unknown", LogLevel.Verbose);
                return UnitReaction.Unknown;
            }*/

            // we need this, to be very accurate, otherwise wow will crash
            wowUnitA.UnitFlags = TrashMem.ReadStruct<BitVector32>(wowUnitA.DescriptorAddress + OffsetList.DescriptorOffsetUnitFlags);
            wowUnitB.UnitFlags = TrashMem.ReadStruct<BitVector32>(wowUnitB.DescriptorAddress + OffsetList.DescriptorOffsetUnitFlags);
            if (wowUnitA.IsDead || wowUnitB.IsDead)
            {
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tCan't get relation, {(wowUnitA.IsDead ? "UnitA is dead..." : "UnitB is dead...")}", LogLevel.Verbose);
                return reaction;
            }

            try
            {

                byte[] reactionBytes = InjectAndExecute(asm, true);
                reaction = (UnitReaction)TrashMem.ReadInt32(memAlloc.Address);

                //reaction = (UnitReaction)BitConverter.ToInt32(reactionBytes, 0);

                SharedCacheManager.Instance.ReactionCache.Add((wowUnitA.FactionTemplate, wowUnitB.FactionTemplate), reaction);
            }
            catch
            {
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X")}]\tFailed to read relation of: {wowUnitA.Name} to {wowUnitB.Name} is Unknown", LogLevel.Verbose);
                return reaction;
            }
            finally
            {
                TrashMem.FreeMemory(memAlloc);
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
