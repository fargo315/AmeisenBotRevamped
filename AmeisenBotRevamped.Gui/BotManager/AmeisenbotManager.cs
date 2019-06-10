using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.AI.CombatEngine.MovementProvider;
using AmeisenBotRevamped.Autologin;
using AmeisenBotRevamped.Autologin.Structs;
using AmeisenBotRevamped.Clients;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.EventAdapters;
using AmeisenBotRevamped.Gui.BotManager.Enums;
using AmeisenBotRevamped.Gui.BotManager.Objects;
using AmeisenBotRevamped.Gui.Views;
using AmeisenBotRevamped.Logging;
using AmeisenBotRevamped.Logging.Enums;
using AmeisenBotRevamped.OffsetLists;
using AmeisenBotRevamped.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using TrashMemCore;

namespace AmeisenBotRevamped.Gui.BotManager
{
    public sealed class AmeisenBotManager
    {
        public bool FleetMode { get; internal set; }

        public List<ManagedAmeisenBot> ManagedAmeisenBots { get; }
        public List<IAmeisenBotView> IAmeisenBotViews { get; }

        private Timer ActiveWowTimer { get; }

        private IOffsetList OffsetList { get; }
        private Settings Settings { get; }
        private Dispatcher Dispatcher { get; }

        private Dictionary<WowAccount, BotStartState> WowAccounts { get; }

        private Queue<WowAccount> WowStartQueue { get; }
        private Queue<WowAccount> LoginQueue { get; }

        public AmeisenBotManager(Dispatcher dispatcher, IOffsetList offsetList, Settings settings)
        {
            AmeisenBotLogger.Instance.Log("Initializing AmeisenBotManager", LogLevel.Verbose);

            Dispatcher = dispatcher;
            OffsetList = offsetList;
            Settings = settings;

            WowAccounts = new Dictionary<WowAccount, BotStartState>();

            WowStartQueue = new Queue<WowAccount>();
            LoginQueue = new Queue<WowAccount>();

            foreach (WowAccount account in JsonConvert.DeserializeObject<List<WowAccount>>(File.ReadAllText(Settings.BotFleetConfig)))
            {
                WowAccounts.Add(account, BotStartState.None);
            }

            IAmeisenBotViews = new List<IAmeisenBotView>();
            ManagedAmeisenBots = new List<ManagedAmeisenBot>();

            ActiveWowTimer = new Timer();
            ActiveWowTimer.Elapsed += ActiveWowTimer_Elapsed;
            ActiveWowTimer.Interval = 1000;
            ActiveWowTimer.Start();
        }

        private void ActiveWowTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                List<WowProcess> wowProcesses = BotUtils.GetRunningWows(OffsetList);
                Dispatcher.Invoke(() => AddUnusedWowProcessesToView(wowProcesses));
                Dispatcher.Invoke(() => AddUsedWowProcessesToView());
                Dispatcher.Invoke(() => RemoveDeadViews());

                ManagedAmeisenBots.RemoveAll(b => b.WowProcess.Process.HasExited);

                if (FleetMode)
                {
                    Dispatcher.Invoke(() => HandleFleet());
                }
            }
            catch (TaskCanceledException)
            {
                // can be ignored, happens sometimes when you exit the bot
                // maybe i'll fix this, maybe not :^)
            }
        }

        private void HandleFleet()
        {
            List<WowProcess> wowsAlreadyRunning = BotUtils.GetRunningWows(OffsetList);

            foreach (WowAccount account in new List<WowAccount>(WowAccounts.Keys))
            {
                switch (WowAccounts[account])
                {
                    case BotStartState.None:
                        WowProcess selectedProcess = wowsAlreadyRunning.FirstOrDefault(w => w.CharacterName == account.CharacterName);

                        if (selectedProcess == null)
                        {
                            selectedProcess = StartNewWowProcess(account);
                        }

                        SetupNewAmeisenBot(account, selectedProcess);
                        break;

                    default:
                        break;
                }
            }
        }

        private WowProcess StartNewWowProcess(WowAccount account)
        {
            List<WowProcess> wowProcesses;
            do
            {
                wowProcesses = BotUtils.GetRunningWows(OffsetList)
                    .Where(p => !p.LoginInProgress
                    && p.CharacterName?.Length == 0).ToList();

                // Start new Wow process
                if (wowProcesses?.Count == 0)
                    StartNewWow(account);
            } while (wowProcesses?.Count == 0);
            return wowProcesses.First();
        }

        private void StartNewWow(WowAccount account)
        {
            WowAccounts[account] = BotStartState.WowStartInProgress;
            Process.Start(Settings.WowExePath).WaitForInputIdle();
            WowAccounts[account] = BotStartState.WowIsRunning;
        }

        private void SetupNewAmeisenBot(WowAccount account, WowProcess wowProcess)
        {
            WowAccounts[account] = BotStartState.BotIsAttaching;

            TrashMem trashMem = new TrashMem(wowProcess.Process);
            MemoryWowDataAdapter memoryWowDataAdapter = new MemoryWowDataAdapter(trashMem, OffsetList);
            MemoryWowActionExecutor memoryWowActionExecutioner = new MemoryWowActionExecutor(trashMem, OffsetList);
            AmeisenNavPathfindingClient ameisenNavPathfindingClient = new AmeisenNavPathfindingClient(Settings.AmeisenNavmeshServerIp, Settings.AmeisenNavmeshServerPort, wowProcess.Process.Id);
            LuaHookWowEventAdapter luaHookWowEventAdapter = new LuaHookWowEventAdapter(memoryWowActionExecutioner);
            BasicMeleeMovementProvider basicMeleeMovementProvider = new BasicMeleeMovementProvider();
            SimpleAutologinProvider simpleAutologinProvider = new SimpleAutologinProvider();

            AmeisenBot ameisenBot = new AmeisenBot(trashMem, memoryWowDataAdapter, simpleAutologinProvider, wowProcess.Process);
            ManagedAmeisenBot managedAmeisenBot = new ManagedAmeisenBot(wowProcess, account, ameisenBot);

            if (ameisenBot.AutologinProvider.DoLogin(wowProcess.Process, account, OffsetList))
            {
                ameisenBot.Attach(memoryWowActionExecutioner, ameisenNavPathfindingClient, luaHookWowEventAdapter, basicMeleeMovementProvider, null);
                if (Settings.WowPositions.ContainsKey(account.CharacterName))
                    ameisenBot.SetWindowPosition(Settings.WowPositions[account.CharacterName]);

                ManagedAmeisenBots.Add(managedAmeisenBot);

                IAmeisenBotViews.OfType<WowView>().ToList().RemoveAll(v => v.WowProcess.Process.Id == managedAmeisenBot.WowProcess.Process.Id);

                WowAccounts[account] = BotStartState.BotIsAttached;
            }
            else
            {
                // we failed to login, restart wow...
                if (!wowProcess.Process.HasExited)
                    wowProcess.Process.Kill();
                WowAccounts[account] = BotStartState.None;
            }
        }

        private void AddUsedWowProcessesToView()
        {
            // Get the used WowProcesses
            foreach (ManagedAmeisenBot managedAmeisenBot in ManagedAmeisenBots)
            {
                BotView botView = new BotView(managedAmeisenBot.AmeisenBot, Settings, AttachAmeisenBot);
                if (!IAmeisenBotViews.Any(v => v.Process.Id == botView.Process.Id))
                    IAmeisenBotViews.Add(botView);
            }
        }

        private void AddUnusedWowProcessesToView(List<WowProcess> wowProcesses)
        {
            // Get the unused WowProcesses
            foreach (WowProcess wowProcess in wowProcesses.Where(p => p.CharacterName?.Length == 0))
            {
                WowView wowView = new WowView(wowProcess);
                if (!IAmeisenBotViews.Any(v => v.Process.Id == wowView.WowProcess.Process.Id))
                    IAmeisenBotViews.Add(wowView);
            }
        }

        private void RemoveDeadViews()
        {
            // remov old wow views converted to a botview
            IAmeisenBotViews.RemoveAll(v => v.GetType() == typeof(WowView) && ManagedAmeisenBots.Any(m => m.WowProcess.Process.Id == v.Process.Id));
            IAmeisenBotViews.RemoveAll(v => v.Process.HasExited);
        }

        private void AttachAmeisenBot(AmeisenBot ameisenBot)
        {

        }

        public void Shutdown()
        {
            foreach (BotView botView in IAmeisenBotViews.Where(a => a.GetType() == typeof(BotView)))
            {
                botView.AmeisenBot.Detach();
            }
        }
    }
}
