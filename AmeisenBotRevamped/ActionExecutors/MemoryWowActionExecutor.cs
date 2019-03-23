using AmeisenBotRevamped.ActionExecutors.Enums;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using AmeisenBotRevamped.OffsetLists;
using AmeisenBotRevamped.Utils;
using Magic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AmeisenBotRevamped.ActionExecutors
{
    public class MemoryWowActionExecutor : IWowActionExecutor
    {
        #region Properties
        public bool IsWoWHooked => BlackMagic.ReadByte(EndsceneAddress) == 0xE9;

        public bool IsWorldLoaded { get; set; }
        #endregion

        #region Internal Properties
        // You may not need them but who knows
        public const uint ENDSCENE_HOOK_OFFSET = 0x2;

        public IOffsetList OffsetList { get; private set; }
        public BlackMagic BlackMagic { get; private set; }

        public byte[] OriginalEndsceneBytes { get; private set; }

        public uint EndsceneAddress { get; private set; }
        public uint EndsceneReturnAddress { get; private set; }
        public uint CodeToExecuteAddress { get; private set; }
        public uint ReturnValueAddress { get; private set; }
        public uint CodecaveForCheck { get; private set; }
        public uint CodecaveForExecution { get; private set; }
        public bool IsInjectionUsed { get; private set; }
        #endregion

        public MemoryWowActionExecutor(BlackMagic blackMagic, IOffsetList offsetList)
        {
            OriginalEndsceneBytes = offsetList.EndSceneBytes;
            BlackMagic = blackMagic;
            OffsetList = offsetList;

            EndsceneAddress = GetEndScene();
            SetupEndsceneHook();
        }

        public void AttackTarget(ulong guid)
        {

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
            return 0;
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
            BlackMagic.WriteFloat(player.BaseAddress + OffsetList.OffsetPlayerRotation, angle);
            SendKey(new IntPtr(0x41), 0, 0); // the "S" key to go a bit backwards TODO: find better method 0x53
        }

        public void SendKey(IntPtr vKey, int minDelay = 20, int maxDelay = 40)
        {
            const uint KEYDOWN = 0x100;
            const uint KEYUP = 0x101;

            IntPtr windowHandle = BlackMagic.WindowHandle;

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
            BlackMagic.WriteUInt64(OffsetList.StaticClickToMoveGuid, guid);
            BlackMagic.WriteInt(OffsetList.StaticClickToMoveAction, (int)clickToMoveType);
        }

        public void MoveToPosition(WowPosition targetPosition, ClickToMoveType clickToMoveType = ClickToMoveType.Move, float distance = 1.5f)
        {
            BlackMagic.WriteFloat(OffsetList.StaticClickToMoveX, targetPosition.x);
            BlackMagic.WriteFloat(OffsetList.StaticClickToMoveY, targetPosition.y);
            BlackMagic.WriteFloat(OffsetList.StaticClickToMoveZ, targetPosition.z);
            BlackMagic.WriteFloat(OffsetList.StaticClickToMoveDistance, distance);
            BlackMagic.WriteInt(OffsetList.StaticClickToMoveAction, (int)clickToMoveType);
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
            if (command.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(command);
                uint argCC = BlackMagic.AllocateMemory(bytes.Length + 1);
                BlackMagic.WriteBytes(argCC, bytes);

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
                BlackMagic.FreeMemory(argCC);
            }
        }

        public string GetLocalizedText(string variable)
        {
            if (variable.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(variable);
                uint argCC = BlackMagic.AllocateMemory(bytes.Length + 1);
                BlackMagic.WriteBytes(argCC, bytes);

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
                BlackMagic.FreeMemory(argCC);
                return result;
            }
            return "";
        }

        private void SetupEndsceneHook()
        {
            // if WoW is already hooked, unhook it
            if (IsWoWHooked) { DisposeHook(); }

            // if WoW is now/was unhooked, hook it
            if (!IsWoWHooked)
            {
                // first thing thats 5 bytes big is here
                // we are going to replace this 5 bytes with
                // our JMP instruction (JMP (1 byte) + Address (4 byte))
                EndsceneAddress += ENDSCENE_HOOK_OFFSET;

                // the address that we will return to after 
                // the jump wer'e going to inject
                EndsceneReturnAddress = EndsceneAddress + 0x5;

                // read our original EndScene
                //OriginalEndsceneBytes = BlackMagic.ReadBytes(EndsceneAddress, 5);

                // integer to check if there is code waiting to be executed
                CodeToExecuteAddress = BlackMagic.AllocateMemory(4);
                BlackMagic.WriteInt(CodeToExecuteAddress, 0);

                // integer to save the address of the return value
                ReturnValueAddress = BlackMagic.AllocateMemory(4);
                BlackMagic.WriteInt(ReturnValueAddress, 0);

                // codecave to check if we need to execute something
                CodecaveForCheck = BlackMagic.AllocateMemory(64);
                // codecave for the code we wa't to execute
                CodecaveForExecution = BlackMagic.AllocateMemory(256);

                BlackMagic.Asm.Clear();
                // save registers
                BlackMagic.Asm.AddLine("PUSHFD");
                BlackMagic.Asm.AddLine("PUSHAD");

                // check for code to be executed
                BlackMagic.Asm.AddLine($"MOV EBX, [{CodeToExecuteAddress}]");
                BlackMagic.Asm.AddLine("TEST EBX, 1");
                BlackMagic.Asm.AddLine("JE @out");

                // execute our stuff and get return address
                BlackMagic.Asm.AddLine($"MOV EDX, {CodecaveForExecution}");
                BlackMagic.Asm.AddLine("CALL EDX");
                BlackMagic.Asm.AddLine($"MOV [{(ReturnValueAddress)}], EAX");

                // finish up our execution
                BlackMagic.Asm.AddLine("@out:");
                BlackMagic.Asm.AddLine("MOV EDX, 0");
                BlackMagic.Asm.AddLine($"MOV [{CodeToExecuteAddress}], EDX");

                // restore registers
                BlackMagic.Asm.AddLine("POPAD");
                BlackMagic.Asm.AddLine("POPFD");

                // needed to determine the position where the original
                // asm is going to be placed
                int asmLenght = BlackMagic.Asm.Assemble().Length;

                // inject the instructions into our codecave
                BlackMagic.Asm.Inject(CodecaveForCheck);
                // ---------------------------------------------------
                // End of the code that checks if there is asm to be
                // executed on our hook
                // ---------------------------------------------------

                // Prepare to replace the instructions inside WoW
                BlackMagic.Asm.Clear();

                // do the original EndScene stuff after we restored the registers
                // and insert it after our code
                BlackMagic.WriteBytes(CodecaveForCheck + (uint)asmLenght, OriginalEndsceneBytes);

                // return to original function after we're done with our stuff
                BlackMagic.Asm.AddLine($"JMP {EndsceneReturnAddress}");
                BlackMagic.Asm.Inject((CodecaveForCheck + (uint)asmLenght) + 5);
                BlackMagic.Asm.Clear();
                // ---------------------------------------------------
                // End of doing the original stuff and returning to
                // the original instruction
                // ---------------------------------------------------

                // modify original EndScene instructions to start the hook
                BlackMagic.Asm.AddLine($"JMP {CodecaveForCheck}");
                BlackMagic.Asm.Inject(EndsceneAddress);
                // we should've hooked WoW now
            }
        }

        private void DisposeHook()
        {
            try
            {
                if (IsWoWHooked)
                {
                    BlackMagic.WriteBytes(EndsceneAddress, OriginalEndsceneBytes);

                    BlackMagic.FreeMemory(CodecaveForCheck);
                    BlackMagic.FreeMemory(CodecaveForExecution);
                    BlackMagic.FreeMemory(CodeToExecuteAddress);
                    BlackMagic.FreeMemory(ReturnValueAddress);
                }
            }
            catch { }
        }

        private uint GetEndScene()
        {
            uint pDevice = BlackMagic.ReadUInt(OffsetList.StaticEndSceneDevice);
            uint pEnd = BlackMagic.ReadUInt(pDevice + OffsetList.EndSceneOffsetDevice);
            uint pScene = BlackMagic.ReadUInt(pEnd);
            return BlackMagic.ReadUInt(pScene + OffsetList.EndSceneOffset);
        }

        public byte[] InjectAndExecute(string[] asm, bool readReturnBytes)
        {
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
                BlackMagic.Asm.Clear();
                // add all lines
                foreach (string s in asm)
                {
                    BlackMagic.Asm.AddLine(s);
                }

                // now there is code to be executed
                BlackMagic.WriteInt(CodeToExecuteAddress, 1);
                // inject it
                BlackMagic.Asm.Inject(CodecaveForExecution);

                // wait for the code to be executed
                while (BlackMagic.ReadInt(CodeToExecuteAddress) > 0) { Thread.Sleep(1); }

                // if we want to read the return value do it otherwise we're done
                if (readReturnBytes)
                {
                    byte buffer = new byte();
                    try
                    {
                        uint dwAddress = BlackMagic.ReadUInt(ReturnValueAddress);

                        // read all parameter-bytes until we the buffer is 0
                        buffer = BlackMagic.ReadByte(dwAddress);
                        while (buffer != 0)
                        {
                            returnBytes.Add(buffer);
                            dwAddress = dwAddress + 1;
                            buffer = BlackMagic.ReadByte(dwAddress);
                        }
                    }
                    catch
                    {
                        // argument reading failed
                    }
                }
            }
            catch
            {
                // now there is no more code to be executed
                BlackMagic.WriteInt(CodeToExecuteAddress, 0);
            }
            IsInjectionUsed = false;

            return returnBytes.ToArray();
        }

        public void AntiAfk() => BlackMagic.WriteInt(OffsetList.StaticTickCount, Environment.TickCount);

        public void Jump() => SendKey(new IntPtr(0x20));

        public void Stop()
        {
            DisposeHook();
        }
    }
}
