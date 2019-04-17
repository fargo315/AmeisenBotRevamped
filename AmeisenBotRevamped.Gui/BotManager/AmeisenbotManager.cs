using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.AI.CombatEngine.MovementProvider;
using AmeisenBotRevamped.Autologin;
using AmeisenBotRevamped.Autologin.Structs;
using AmeisenBotRevamped.Clients;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.EventAdapters;
using AmeisenBotRevamped.Gui.BotManager.Enums;
using AmeisenBotRevamped.Gui.BotManager.Objects;
using AmeisenBotRevamped.Logging;
using AmeisenBotRevamped.Logging.Enums;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using AmeisenBotRevamped.OffsetLists;
using AmeisenBotRevamped.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TrashMemCore;

namespace AmeisenBotRevamped.Gui.BotManager
{
    public sealed class AmeisenBotManager
    {
        public bool IsActive { get; private set; }
        public bool Enabled { get; set; }

        public Settings Settings { get; set; }
        public IOffsetList OffsetList { get; set; }

        public List<Thread> AmeisenBotThreads { get; private set; }
        public List<Thread> AmeisenBotWatchdogThreads { get; private set; }

        private List<WowProcess> ActiveWowProcesses { get; set; }
        public List<ManagedAmeisenBot> ManagedAmeisenBots { get; set; }

        private Thread WowDispatcher { get; set; }
        private ConcurrentQueue<WowAccount> AmeisenBotQueue { get; set; }

        private List<WowAccount> WowAccounts { get; set; }
        private Dictionary<WowAccount, StartState> WowAccountsRunning { get; set; }

        public AmeisenBotManager(Settings settings, List<WowAccount> wowAccounts, IOffsetList offsetList)
        {
            AmeisenBotLogger.Instance.Log("Initializing AmeisenBotManager", LogLevel.Verbose);

            Enabled = false;
            Init(settings, wowAccounts, offsetList);
        }

        private void Init(Settings settings, List<WowAccount> wowAccounts, IOffsetList offsetList)
        {
            IsActive = true;

            Settings = settings;
            WowAccounts = wowAccounts;
            OffsetList = offsetList;

            WowDispatcher = new Thread(new ThreadStart(RunWowDispatcher));
            AmeisenBotThreads = new List<Thread>();
            AmeisenBotWatchdogThreads = new List<Thread>();

            ActiveWowProcesses = new List<WowProcess>();
            ManagedAmeisenBots = new List<ManagedAmeisenBot>();

            AmeisenBotQueue = new ConcurrentQueue<WowAccount>();

            WowAccountsRunning = new Dictionary<WowAccount, StartState>();

            foreach (WowAccount account in wowAccounts)
            {
                WowAccountsRunning.Add(account, StartState.NotRunning);
                AmeisenBotQueue.Enqueue(account);
            }

            // start the dispatcher
            WowDispatcher.Start();
        }

        public void Dispose()
        {
            IsActive = false;

            foreach (ManagedAmeisenBot bot in ManagedAmeisenBots)
            {
                bot.AmeisenBot.Detach();
            }

            foreach (Thread t in AmeisenBotThreads)
            {
                t.Abort();
                t.Join();
            }

            foreach (Thread t in AmeisenBotWatchdogThreads)
            {
                t.Join();
            }

            WowDispatcher.Join();
        }

        public void Start() => Enabled = true;
        public void Stop() => Enabled = false;

        private void RunWowDispatcher()
        {
            while (IsActive)
            {
                try
                {
                    if (Enabled)
                    {
                        if (AmeisenBotQueue.TryDequeue(out WowAccount wowAccount)
                            && WowAccountsRunning.TryGetValue(wowAccount, out StartState state)
                            && state == StartState.NotRunning)
                        {
                            // The bot is now going to be started
                            WowAccountsRunning[wowAccount] = StartState.StartInProgress;

                            WowProcess wowProcess = FindUnusedWowProcess();
                            DoLogin(wowProcess, wowAccount);
                        }

                        RemoveDeadProcesses();
                    }
                }
                catch (Exception ex)
                {
                    AmeisenBotLogger.Instance.Log($"Error at Thread: WowDispatcher: \n{ex}", LogLevel.Error);
                }

                Thread.Sleep(1000);
            }
        }

        private WowProcess FindUnusedWowProcess()
        {
            // Get all WowProcesses that are free to use
            IEnumerable<WowProcess> unusedWowProcesses =
                ActiveWowProcesses.Where(
                    w => !w.LoginInProgress
                    && w.CharacterName.Length == 0);

            WowProcess wowProcessToUse = null;
            // Search for a useable wow process
            do
            {
                // If there is no free Process, start a new one
                if (!unusedWowProcesses.Any())
                {
                    Process newWowProcess = StartWowProcess();
                    // after we started the wow, rescan all processes
                    ActiveWowProcesses = BotUtils.GetRunningWows(OffsetList);
                }
                else
                {
                    wowProcessToUse = unusedWowProcesses.First();
                }
            } while (wowProcessToUse == null);
            return wowProcessToUse;
        }

        private void DoLogin(WowProcess wowProcess, WowAccount wowAccount)
        {
            // now we are going to attach and use this process
            wowProcess.LoginInProgress = true;
            AmeisenBot bot = SetupAmeisenBot(wowProcess.Process);

            // create a thred for the Bot to run on
            Thread ameisenbotThread = new Thread(new ThreadStart(() => AttachAmeisenBot(bot)));
            AmeisenBotThreads.Add(ameisenbotThread);
            ManagedAmeisenBot managedAmeisenBot = new ManagedAmeisenBot(ameisenbotThread, bot, wowAccount);

            // start the watchdog that will restart the wow when it crashed
            Thread watchdogThread = new Thread(new ThreadStart(() => RunWowWatchdog(managedAmeisenBot)));
            AmeisenBotWatchdogThreads.Add(watchdogThread);

            // start the thread and its watchdog
            ameisenbotThread.Start();
            watchdogThread.Start();
        }

        public AmeisenBot SetupAmeisenBot(Process wowProcess)
        {
            AmeisenBotLogger.Instance.Log($"[{wowProcess.Id.ToString("X")}]\tSetting up the AmeisenBot...");

            TrashMem trashMem = new TrashMem(wowProcess);
            IAutologinProvider autologinProvider = new SimpleAutologinProvider();
            IWowDataAdapter wowDataAdapter = new MemoryWowDataAdapter(trashMem, OffsetList);

            return new AmeisenBot(trashMem, wowDataAdapter, autologinProvider, wowProcess);
        }

        public void AttachAmeisenBotOnNewThread(AmeisenBot bot)
        {
            if (bot.Attached)
            {
                bot.Detach();
            }
            else
            {
                Thread ameisenbotThread = new Thread(new ThreadStart(() => AttachAmeisenBot(bot)));
                AmeisenBotThreads.Add(ameisenbotThread);
                ameisenbotThread.Start();
            }
        }

        private void AttachAmeisenBot(AmeisenBot bot)
        {
            IWowActionExecutor wowActionExecutor = new MemoryWowActionExecutor(bot.TrashMem, OffsetList);
            IPathfindingClient pathfindingClient = new AmeisenNavPathfindingClient(Settings.AmeisenNavmeshServerIp, Settings.AmeisenNavmeshServerPort, bot.Process.Id);
            IWowEventAdapter wowEventAdapter = new LuaHookWowEventAdapter(wowActionExecutor);
            bot.Attach(wowActionExecutor, pathfindingClient, wowEventAdapter, new BasicMeleeMovementProvider(), null);
        }

        private Process StartWowProcess()
        {
            Process newWowProcess = Process.Start(Settings.WowExePath);
            AmeisenBotLogger.Instance.Log($"[{newWowProcess.Id.ToString("X")}]\tStarting new WoW process..");
            newWowProcess.WaitForInputIdle();
            return newWowProcess;
        }

        private void RunWowWatchdog(ManagedAmeisenBot bot)
        {
            bool isMyWowCrashed = false;
            while (IsActive && !isMyWowCrashed)
            {
                try
                {
                    if (Enabled)
                    {
                        if (bot.AmeisenBot.WowDataAdapter.GameState == WowGameState.Crashed)
                        {
                            // in case of a crash, restart the wow and watchdog
                            bot.AmeisenBot.Detach();
                            bot.Thread.Abort();

                            WowAccountsRunning[bot.WowAccount] = StartState.NotRunning;
                            AmeisenBotQueue.Enqueue(bot.WowAccount);
                            isMyWowCrashed = true;
                        }
                        else if (bot.AmeisenBot.WowDataAdapter.GameState == WowGameState.World)
                        {
                            WowAccountsRunning[bot.WowAccount] = StartState.Running;
                        }
                    }
                }
                catch (Exception ex)
                {
                    AmeisenBotLogger.Instance.Log($"Error at Thread: WowWatchdog: \n{ex}", LogLevel.Error);
                }

                Thread.Sleep(1000);
            }
        }

        private void RemoveDeadProcesses()
        {
            List<WowProcess> aliveProcesses = new List<WowProcess>();
            foreach (WowProcess p in ActiveWowProcesses)
            {
                if (p.Process.HasExited)
                {
                    aliveProcesses.Add(p);
                }
            }
            ActiveWowProcesses = aliveProcesses;
        }
    }
}
