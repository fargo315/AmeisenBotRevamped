using AmeisenBotRevamped.Gui.BotManager.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaktionslogik für DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private List<ManagedAmeisenBot> ManagedAmeisenBots { get; }

        public DebugWindow(List<ManagedAmeisenBot> managedAmeisenBots)
        {
            ManagedAmeisenBots = managedAmeisenBots;
            InitializeComponent();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (ManagedAmeisenBot ameisenBot in ManagedAmeisenBots)
                comboboxBots.Items.Add(ameisenBot);

                comboboxBots.Items.Add("gg");
            comboboxBots.Items.Add("gg2");
            comboboxBots.Items.Add("gg4");
        }
    }
}
