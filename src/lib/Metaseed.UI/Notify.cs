using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Metaseed.MetaKeyboard
{
    public class Notify
    {
        private static NotifyIcon trayIcon;

        static Notify()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = new System.Drawing.Icon("./metaKeyboard.ico"),
                ContextMenu = new ContextMenu(new MenuItem[]
                {
                    new MenuItem("Exit", Exit),
                    new MenuItem("Show Log", ( o, e) =>
                    {
                        var menu = o as MenuItem;
                        if (menu.Checked)
                        {
                            menu.Checked = false;
                            menu.Text = "Show Log";
                            UI.Window.HideConsole();
                        }
                        else
                        {
                            menu.Checked = true;
                            menu.Text = "Hide Log";

                            UI.Window.ShowConsole();
                        }
                    }) {Checked = false}
                }),
                Visible = true
            };
        }

        public static void Show(string description)
        {
            if(description=="") return;
            trayIcon.ShowBalloonTip(3000000, "", description, ToolTipIcon.None);
        }

        static void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;

            Application.Exit();
        }
    }
}