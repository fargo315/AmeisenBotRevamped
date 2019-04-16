using AmeisenBotRevamped.Autologin.Structs;
using AmeisenBotRevamped.Logging;
using AmeisenBotRevamped.Logging.Enums;
using AmeisenBotRevamped.OffsetLists;
using TrashMemCore;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using AmeisenBotRevamped.ActionExecutors;

namespace AmeisenBotRevamped.Autologin
{
    public class SimpleAutologinProvider : IAutologinProvider
    {
        public const uint KEYDOWN = 0x100;
        public const uint KEYUP = 0x101;
        public const uint WM_CHAR = 0x0102;

        private IOffsetList OffsetList { get; set; }

        public bool LoginInProgress { get; private set; }

        public string LoginInProgressCharactername { get; private set; }

        public void DoLogin(Process process, WowAccount wowAccount, IOffsetList offsetlist)
        {
            try
            {
                TrashMem trashMem = new TrashMem(process);
                int count = 0;

                LoginInProgress = true;
                LoginInProgressCharactername = wowAccount.CharacterName;

                OffsetList = offsetlist;

                while (trashMem.ReadInt32(offsetlist.StaticIsWorldLoaded) != 1 && count < 8)
                {
                    switch (trashMem.ReadString(offsetlist.StaticGameState, Encoding.ASCII, 10))
                    {
                        case "login":
                            HandleLogin(trashMem, process, wowAccount);
                            break;

                        case "charselect":
                            HandleCharSelect(trashMem, process, wowAccount);
                            break;

                        default:
                            break;
                    }

                    Thread.Sleep(2000);
                    count++;
                }
            }
            catch (Exception e)
            {
                AmeisenBotLogger.Instance.Log($"[{process.Id.ToString("X")}]\tCrash at Login: \n{e.ToString()}", LogLevel.Error);
            }

            AmeisenBotLogger.Instance.Log($"[{process.Id.ToString("X")}]\tLogin successful...", LogLevel.Verbose);
            LoginInProgress = false;
            LoginInProgressCharactername = "";
        }

        private void HandleLogin(TrashMem trashMem, Process process, WowAccount wowAccount)
        {
            AmeisenBotLogger.Instance.Log($"[{process.Id.ToString("X")}]\tHandling Login into account: {wowAccount.Username}:{wowAccount.CharacterName}:{wowAccount.CharacterSlot}", LogLevel.Verbose);
            foreach (char c in wowAccount.Username)
            {
                SendKeyToProcess(process, c, char.IsUpper(c));
                Thread.Sleep(10);
            }

            Thread.Sleep(100);
            SendKeyToProcess(process, 0x09);
            Thread.Sleep(100);

            bool firstTime = true;
            do
            {
                if (!firstTime)
                {
                    SendKeyToProcess(process, 0x0D);
                }

                foreach (char c in wowAccount.Password)
                {
                    SendKeyToProcess(process, c, char.IsUpper(c));
                    Thread.Sleep(10);
                }

                Thread.Sleep(500);
                SendKeyToProcess(process, 0x0D);
                Thread.Sleep(3000);

                firstTime = false;
            } while (trashMem.ReadString(OffsetList.StaticGameState, Encoding.ASCII,10) == "login");
        }

        private void HandleCharSelect(TrashMem trashMem, Process process, WowAccount wowAccount)
        {
            AmeisenBotLogger.Instance.Log($"[{process.Id.ToString("X")}]\tHandling Characterselection: {wowAccount.Username}:{wowAccount.CharacterName}:{wowAccount.CharacterSlot}", LogLevel.Verbose);
            int currentSlot = trashMem.ReadInt32(OffsetList.StaticCharacterSlotSelected);

            while (currentSlot != wowAccount.CharacterSlot)
            {
                SendKeyToProcess(process, 0x28);
                Thread.Sleep(200);
                currentSlot = trashMem.ReadInt32(OffsetList.StaticCharacterSlotSelected);
            }

            SendKeyToProcess(process, 0x0D);
        }

        private static void SendKeyToProcess(Process process, int c)
        {
            IntPtr windowHandle = process.MainWindowHandle;

            SafeNativeMethods.SendMessage(windowHandle, KEYDOWN, new IntPtr(c), new IntPtr(0));
            Thread.Sleep(new Random().Next(20, 40));
            SafeNativeMethods.SendMessage(windowHandle, KEYUP, new IntPtr(c), new IntPtr(0));
        }

        private static void SendKeyToProcess(Process process, int c, bool shift)
        {
            IntPtr windowHandle = process.MainWindowHandle;

            if (shift)
            {
                SafeNativeMethods.PostMessage(windowHandle, KEYDOWN, new IntPtr(0x10), new IntPtr(0));
            }

            SafeNativeMethods.PostMessage(windowHandle, WM_CHAR, new IntPtr(c), new IntPtr(0));

            if (shift)
            {
                SafeNativeMethods.PostMessage(windowHandle, KEYUP, new IntPtr(0x10), new IntPtr(0));
            }
        }
    }
}
