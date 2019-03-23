using AmeisenBotRevamped.OffsetLists;
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

        public MainWindow()
        {
            InitializeComponent();

            ViewUpdateTimer = new Timer(1000);
            ViewUpdateTimer.Elapsed += CUpdateViews;
        }

        private void CUpdateViews(object sender, ElapsedEventArgs e)
        {
            foreach (BotView botView in BotViews)
            {
                try
                {
                    Dispatcher.Invoke(() => botView.UpdateView());
                }
                catch { }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AmeisenBots = new List<AmeisenBot>();
            BotViews = new List<BotView>();

            ScanForWows();

            foreach (AmeisenBot ameisenBot in AmeisenBots)
            {
                BotView botview = new BotView(ameisenBot);
                BotViews.Add(botview);
                mainWrappanel.Children.Add(botview);

                AttachBot(ameisenBot);
            }

            ViewUpdateTimer.Start();

            WowAccount wowAccount = new WowAccount()
        }

        private void ScanForWows()
        {
            foreach (WowProcess wowProcess in BotUtils.GetRunningWows(new Wotlk335a12340OffsetList()))
            {
                AmeisenBots.Add(SetupAmeisenBot(wowProcess));
            }
        }

        private void AttachBot(AmeisenBot ameisenBot)
        {
            IWowActionExecutor wowActionExecutor = new MemoryWowActionExecutor(ameisenBot.BlackMagic, new Wotlk335a12340OffsetList());
            IPathfindingClient pathfindingClient = new AmeisenNavPathfindingClient("127.0.0.1", 47110);
            //IWowEventAdapter wowEventAdapter = new MemoryWowEventAdapter(wowActionExecutor);
            ameisenBot.Attach(wowActionExecutor, pathfindingClient, null);
        }

        private AmeisenBot SetupAmeisenBot(WowProcess wowProcess)
        {
            BlackMagic blackMagic = new BlackMagic(wowProcess.Process.Id);
            Wotlk335a12340OffsetList offsetList = new Wotlk335a12340OffsetList();

            IAutologinProvider autologinProvider = new SimpleAutologinProvider();
            IWowDataAdapter wowDataAdapter = new MemoryWowDataAdapter(blackMagic, offsetList);

            return new AmeisenBot(blackMagic, wowDataAdapter, autologinProvider, wowProcess.Process);
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (AmeisenBot ameisenBot in AmeisenBots)
            {
                ameisenBot.Detach();
            }
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    }
}
