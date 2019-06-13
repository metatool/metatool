using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Metaseed.Input;
using Metaseed.UI;

namespace ConsoleApp1
{
    public class FunctionalKeys
    {
        public FunctionalKeys()
        {
            Keys.F.With(Keys.LWin).Down("", "", e =>
            {
                var c = UI.CurrentWindowClass;
                if ("CabinetWClass" == c)
                {
                    using (var automation = new UIA3Automation())
                    {
                        var h = UI.CurrentWindowHandle;
                        var ea = automation.FromHandle(h);
                        var a = ea.FindFirstDescendant(cf => cf.ByClassName("UIItem"))?.AsListBoxItem().Select();
                    }
                }

                e.Handled = true;
            });
        }
    }
}
