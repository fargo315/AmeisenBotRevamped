using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AmeisenBotRevamped.Autologin.Structs;
using AmeisenBotRevamped.OffsetLists;
using Magic;

namespace AmeisenBotRevamped.Autologin
{
    public class SimpleAutologinProvider : IAutologinProvider
    {
        public const uint KEYDOWN = 0x100;
        public const uint KEYUP = 0x101;
        public const uint WM_CHAR = 0x0102;

        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private IOffsetList OffsetList { get; set; }

        public bool LoginInProgress { get; private set; }

        public string LoginInProgressCharactername { get; private set; }

        public void DoLogin(Process process, WowAccount wowAccount, IOffsetList offsetlist)
        {
            try
            {
                BlackMagic blackMagic = new BlackMagic(process.Id);
                int count = 0;

                LoginInProgress = true;
                LoginInProgressCharactername = wowAccount.CharacterName;

                OffsetList = offsetlist;

                while (blackMagic.ReadInt(offsetlist.StaticIsWorldLoaded) != 1 && count < 8)
                {
                    switch (blackMagic.ReadASCIIString(offsetlist.StaticGameState, 10))
                    {
                        case "login":
                            HandleLogin(blackMagic, process, wowAccount);
                            break;

                        case "charselect":
                            HandleCharSelect(blackMagic, process, wowAccount);
                            break;

                        default:
                            break;
                    }

                    Thread.Sleep(2000);
                    count++;
                }
            }
            catch { }

            LoginInProgress = false;
            LoginInProgressCharactername = "";
        }

        private void HandleLogin(BlackMagic blackMagic, Process process, WowAccount wowAccount)
        {
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
            } while (blackMagic.ReadASCIIString(OffsetList.StaticGameState, 10) == "login");
        }

        private void HandleCharSelect(BlackMagic blackMagic, Process process, WowAccount wowAccount)
        {
            int currentSlot = blackMagic.ReadInt((uint)blackMagic.MainModule.BaseAddress + OffsetList.StaticCharacterSlotSelected);

            while (currentSlot != wowAccount.CharacterSlot)
            {
                SendKeyToProcess(process, 0x28);
                Thread.Sleep(200);
                currentSlot = blackMagic.ReadInt((uint)blackMagic.MainModule.BaseAddress + OffsetList.StaticCharacterSlotSelected);
            }

            SendKeyToProcess(process, 0x0D);
        }

        private static void SendKeyToProcess(Process process, int c)
        {
            IntPtr windowHandle = process.MainWindowHandle;

            SendMessage(windowHandle, KEYDOWN, new IntPtr(c), new IntPtr(0));
            Thread.Sleep(new Random().Next(20, 40));
            SendMessage(windowHandle, KEYUP, new IntPtr(c), new IntPtr(0));
        }

        private static void SendKeyToProcess(Process process, int c, bool shift)
        {
            IntPtr windowHandle = process.MainWindowHandle;

            if (shift)
            {
                PostMessage(windowHandle, KEYDOWN, new IntPtr(0x10), new IntPtr(0));
            }

            PostMessage(windowHandle, WM_CHAR, new IntPtr(c), new IntPtr(0));

            if (shift)
            {
                PostMessage(windowHandle, KEYUP, new IntPtr(0x10), new IntPtr(0));
            }
        }
    }
}
