using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.Gui.Views
{
    public interface IAmeisenBotView
    {
        Process Process { get; }
        void UpdateView();
    }
}
