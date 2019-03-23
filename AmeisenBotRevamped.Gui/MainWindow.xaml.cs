﻿using AmeisenBotRevamped.OffsetLists;
using AmeisenBotRevamped.Utils;
using AmeisenBotRevamped;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AmeisenBotRevamped.Gui.Views;
using AmeisenBotRevamped.DataAdapters;
using Magic;
using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.Clients;
using AmeisenBotRevamped.EventAdapters;
using System.Timers;
using AmeisenBotRevamped.Autologin;
using AmeisenBotRevamped.Autologin.Structs;
using Newtonsoft.Json;
using System.IO;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;

namespace AmeisenBotRevamped.Gui
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<AmeisenBot> AmeisenBots { get; set; }
        private List<BotView> BotViews { get; set; }

        private Timer ViewUpdateTimer { get; set; }
        private Timer BotFleetTimer { get; set; }

        private Settings Settings { get; set; }

        private Dictionary<string, bool> WowStartupMap { get; set; }

        private string SettingsPath => AppDomain.CurrentDomain.BaseDirectory + "config.json";

        public MainWindow()
        {
            InitializeComponent();

            WowStartupMap = new Dictionary<string, bool>();

            ViewUpdateTimer = new Timer(1000);
            ViewUpdateTimer.Elapsed += CUpdateViews;

            BotFleetTimer = new Timer(1000);
            BotFleetTimer.Elapsed += CBotFleetTimer;

            if (File.Exists(SettingsPath))
            {
                Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsPath));
            }
            else
            {
                Settings = new Settings() { BotListFilePath = "", WowExecutableFilePath = "" };
                File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(Settings));
            }
        }

        private void CBotFleetTimer(object sender, ElapsedEventArgs e)
        {
            CheckForBotFleet();
        }

        private void CUpdateViews(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (BotView botView in BotViews)
                {
                    Dispatcher.Invoke(() => botView.UpdateView());
                }
            }
            catch { }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AmeisenBots = new List<AmeisenBot>();
            BotViews = new List<BotView>();

            ScanForWows();
            RefreshActiveWows();

            ViewUpdateTimer.Start();
            BotFleetTimer.Start();

            //CheckForBotFleet();
        }

        private void RefreshActiveWows()
        {
            mainWrappanel.Children.Clear();
            BotViews = new List<BotView>();

            foreach (AmeisenBot ameisenBot in AmeisenBots)
            {
                if(ameisenBot.WowDataAdapter.GameState == WowGameState.Crashed)
                {
                    ameisenBot.Detach();
                    GC.Collect();

                    WowStartupMap[ameisenBot.CharacterName] = false;
                    AmeisenBots.Remove(ameisenBot);
                    break;
                }

                AddBotToView(ameisenBot);

                if (!ameisenBot.Attached)
                    AttachBot(ameisenBot);
            }
        }

        private void AddBotToView(AmeisenBot ameisenBot)
        {
            BotView botview = new BotView(ameisenBot);

            if (!BotViews.Contains(botview))
            {
                BotViews.Add(botview);
                mainWrappanel.Children.Add(botview);
            }
        }

        private void CheckForBotFleet()
        {
            if (Settings.BotListFilePath != "" && File.Exists(Settings.BotListFilePath))
            {
                List<WowAccount> accounts = JsonConvert.DeserializeObject<List<WowAccount>>(File.ReadAllText(Settings.BotListFilePath));
                foreach (WowAccount wowAccount in accounts)
                {
                    if (!WowStartupMap.ContainsKey(wowAccount.CharacterName))
                        WowStartupMap.Add(wowAccount.CharacterName, false);

                    if (!CharacterIsLoggedIn(wowAccount.CharacterName)
                        && !LoginForCharacterIsInProgress(wowAccount.CharacterName)
                        && !WowStartupMap[wowAccount.CharacterName])
                    {
                        List<AmeisenBot> avaiableBots = AmeisenBots.Where(
                            bot => bot.CharacterName == ""
                            && !bot.AutologinProvider.LoginInProgress
                            && bot.WowDataAdapter.GameState != WowGameState.Crashed
                        ).ToList();

                        AmeisenBot ameisenBot = null;

                        if (avaiableBots.Count < 1)
                        {
                            if (Settings.WowExecutableFilePath != "" && File.Exists(Settings.WowExecutableFilePath))
                            {
                                WowStartupMap[wowAccount.CharacterName] = true;

                                Process newWowProcess = Process.Start(Settings.WowExecutableFilePath);
                                newWowProcess.WaitForInputIdle();

                                ameisenBot = SetupAmeisenBot(newWowProcess);
                                AmeisenBots.Add(ameisenBot);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            ameisenBot = avaiableBots.First();
                        }

                        if (ameisenBot == null) continue;

                        ameisenBot.AutologinProvider.DoLogin(ameisenBot.Process, wowAccount, new Wotlk335a12340OffsetList());
                        ameisenBot.ClearCaches();

                        try
                        {
                            Dispatcher.Invoke(() => AddBotToView(ameisenBot));
                        }
                        catch { }

                        WowStartupMap[ameisenBot.CharacterName] = false;
                    }
                }
            }

            try
            {
                Dispatcher.Invoke(() => RefreshActiveWows());
            }
            catch { }
        }

        private bool LoginForCharacterIsInProgress(string characterName)
        {
            foreach (AmeisenBot bot in AmeisenBots) if (bot.AutologinProvider.LoginInProgressCharactername == characterName) return true;
            return false;
        }

        private bool CharacterIsLoggedIn(string characterName)
        {
            foreach (AmeisenBot bot in AmeisenBots) if (bot.CharacterName == characterName) return true;
            return false;
        }

        private void ScanForWows()
        {
            List<AmeisenBot> AmeisenBotsNew = new List<AmeisenBot>();
            foreach (WowProcess wowProcess in BotUtils.GetRunningWows(new Wotlk335a12340OffsetList()))
            {
                AmeisenBotsNew.Add(SetupAmeisenBot(wowProcess.Process));
            }
            AmeisenBots = AmeisenBotsNew;
        }

        private void AttachBot(AmeisenBot ameisenBot)
        {
            IWowActionExecutor wowActionExecutor = new MemoryWowActionExecutor(ameisenBot.BlackMagic, new Wotlk335a12340OffsetList());
            IPathfindingClient pathfindingClient = new AmeisenNavPathfindingClient("127.0.0.1", 47110);
            //IWowEventAdapter wowEventAdapter = new MemoryWowEventAdapter(wowActionExecutor);
            ameisenBot.Attach(wowActionExecutor, pathfindingClient, null);
        }

        private AmeisenBot SetupAmeisenBot(Process wowProcess)
        {
            BlackMagic blackMagic = new BlackMagic(wowProcess.Id);
            Wotlk335a12340OffsetList offsetList = new Wotlk335a12340OffsetList();

            IAutologinProvider autologinProvider = new SimpleAutologinProvider();
            IWowDataAdapter wowDataAdapter = new MemoryWowDataAdapter(blackMagic, offsetList);

            return new AmeisenBot(blackMagic, wowDataAdapter, autologinProvider, wowProcess);
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewUpdateTimer.Stop();
            BotFleetTimer.Stop();

            foreach (AmeisenBot ameisenBot in AmeisenBots)
            {
                ameisenBot.Detach();
            }
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            CheckForBotFleet();
        }
    }
}