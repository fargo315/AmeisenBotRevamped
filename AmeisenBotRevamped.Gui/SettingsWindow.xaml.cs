using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AmeisenBotRevamped.Gui
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private List<AmeisenBot> AmeisenBots { get; }
        public Settings Settings { get; }
        public Timer ViewUpdateTimer { get; private set; }

        public AmeisenBot SelectedBot => (AmeisenBot)listboxBots.SelectedItem;

        public SettingsWindow(Settings activeSettings, List<AmeisenBot> ameisenBots)
        {
            Settings = activeSettings;
            AmeisenBots = ameisenBots;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (AmeisenBot ameisenBot in AmeisenBots)
            {
                listboxBots.Items.Add(ameisenBot);
            }

            ViewUpdateTimer = new Timer(1000);
            ViewUpdateTimer.Elapsed += CUpdateViews;
            ViewUpdateTimer.Start();

            labelWowExePath.Content = $"{Settings.WowExePath}";
            labelBotPictureFolder.Content = $"{Settings.BotPictureFolder}";
            labelBotFleetConfig.Content = $"{Settings.BotFleetConfig}";
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CUpdateViews(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() => UpdateBotView());
            }
            catch { }
        }

        private void UpdateBotView()
        {
            if (listboxBots.SelectedItem != null)
            {
                UpdateWindowPositions();
            }
        }

        private void ListboxBots_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            labelBotName.Content = $"{SelectedBot}";
            UpdateWindowPositions();
        }

        private void ButtonApplySavedWindowPosition_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.WowPositions.ContainsKey(SelectedBot.CharacterName))
                SelectedBot.SetWindowPosition(Settings.WowPositions[SelectedBot.CharacterName]);
        }

        private void ButtonSaveWindowPosition_Click(object sender, RoutedEventArgs e)
        {
            AddOrReplaceRect(SelectedBot.GetWindowPosition());
        }

        private void ButtonResetWindowPosition_Click(object sender, RoutedEventArgs e)
        {
            AddOrReplaceRect(new ActionExecutors.Structs.Rect());
            SelectedBot.SetWindowPosition(new ActionExecutors.Structs.Rect());
        }

        private void AddOrReplaceRect(ActionExecutors.Structs.Rect rect)
        {
            if (Settings.WowPositions.ContainsKey(SelectedBot.CharacterName))
                Settings.WowPositions[SelectedBot.CharacterName] = rect;
            else
                Settings.WowPositions.Add(SelectedBot.CharacterName, rect);
        }

        private void UpdateWindowPositions()
        {
            labelWindowPosition.Content = $"{JsonConvert.SerializeObject(SelectedBot.GetWindowPosition())}";

            if (Settings.WowPositions.ContainsKey(SelectedBot.CharacterName))
                labelSavedWindowPosition.Content = $"{JsonConvert.SerializeObject(Settings.WowPositions[SelectedBot.CharacterName])}";
            else
                labelSavedWindowPosition.Content = $"{JsonConvert.SerializeObject(new ActionExecutors.Structs.Rect())}";
        }

        private void ButtonWowExePath_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Settings.WowExePath = openFileDialog.FileName;
                labelWowExePath.Content = $"{Settings.WowExePath}";
            }
        }

        private void ButtonBotPictureFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Settings.BotPictureFolder = openFileDialog.FileName;
                labelBotPictureFolder.Content = $"{Settings.BotPictureFolder}";
            }
        }

        private void ButtonBotFleetConfig_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Settings.BotFleetConfig = openFileDialog.FileName;
                labelBotFleetConfig.Content = $"{Settings.BotFleetConfig}";
            }
        }

        private void TextboxNavmeshServerIp_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIpInput();
        }

        private void CheckIpInput()
        {
            if (IPAddress.TryParse(textboxNavmeshServerIp.Text, out IPAddress ip))
            {
                if (labelNavmeshServerPortCheck != null)
                {
                    Settings.AmeisenNavmeshServerIp = ip.ToString();
                    labelNavmeshServerIpCheck.Content = "✔️";
                    labelNavmeshServerIpCheck.Foreground = new SolidColorBrush(Colors.LawnGreen);
                }
            }
            else
            {
                if (labelNavmeshServerPortCheck != null)
                {
                    labelNavmeshServerIpCheck.Content = "❌";
                    labelNavmeshServerIpCheck.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void TextboxNavmeshServerPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckPortInput();
        }

        private void CheckPortInput()
        {
            if (int.TryParse(textboxNavmeshServerPort.Text, out int port))
            {
                Settings.AmeisenNavmeshServerPort = port;
                if (labelNavmeshServerPortCheck != null)
                {
                    labelNavmeshServerPortCheck.Content = "✔️";
                    labelNavmeshServerPortCheck.Foreground = new SolidColorBrush(Colors.LawnGreen);
                }
            }
            else
            {
                if (labelNavmeshServerPortCheck != null)
                {
                    labelNavmeshServerPortCheck.Content = "❌";
                    labelNavmeshServerPortCheck.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void TextboxNavmeshServerIp_Loaded(object sender, RoutedEventArgs e)
        {
            textboxNavmeshServerIp.Text = $"{Settings.AmeisenNavmeshServerIp}";
            CheckIpInput();
        }

        private void TextboxNavmeshServerPort_Loaded(object sender, RoutedEventArgs e)
        {
            textboxNavmeshServerPort.Text = $"{Settings.AmeisenNavmeshServerPort}";
            CheckPortInput();
        }
    }
}
