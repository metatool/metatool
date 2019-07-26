﻿using System;
using System.Reflection;
using System.Windows;
using ConsoleApp1;
using Metaseed.Core;
using Metaseed.Input;
using Metaseed.NotifyIcon;
using Microsoft.Win32;

namespace Metaseed.MetaKeyboard
{
    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Application.Current.MainWindow = new MainWindow();
            Notify.ShowMessage("MetaKeyboard started!");

            UI.Window.InitialConsole();
            Notify.AddContextMenuItem("Show Log", e =>
            {
                if (e.IsChecked)
                {
                    UI.Window.ShowConsole();
                }
                else
                {
                    UI.Window.HideConsole();
                }
            }, null, true);

            Notify.AddContextMenuItem("Auto Start", e => AutoStartManager.IsAutoStart = e.IsChecked, null, true,
                AutoStartManager.IsAutoStart);

            Keyboard.KeyPress += (o, e1) => { };


            var keyboard61 = new Keyboard61();
            var mouse      = new Mouse();
            var fun        = new FunctionalKeys();

            var software = new Utilities();
            // var c = new ClipboardManager();

            Keyboard.Hook();
        }
    }
}