using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Metaseed.NotifyIcon;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Metaseed.UI.Notify;

namespace Metaseed.MetaKeyboard
{
    public class Notify
    {
        private static TaskbarIcon trayIcon;

        static Notify()
        {
           var foo = new Uri("pack://application:,,,/Metaseed.UI;component/Notify/NotifyIconResources.xaml", UriKind.RelativeOrAbsolute);
           Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = foo });
           trayIcon = Application.Current.FindResource("NotifyIcon") as TaskbarIcon;

          }

        public static void AddContextMenuItem(string header, Action<MenuItem> excute, Func<MenuItem, bool> canExcute=null, bool isCheckable = false)
        {
            var item = new MenuItem(){Header = header, IsCheckable =  isCheckable};
            item.Command = new DelegateCommand<MenuItem>(){CanExecuteFunc = canExcute,CommandAction = excute};
            item.CommandParameter = item;
            trayIcon.ContextMenu.Items.Insert(0,item);
        }

        public static void ShowMessage(string msg)
        {
            if (msg == "") return;
            trayIcon.ShowBalloonTip(string.Empty, msg, BalloonIcon.None);
        }
        public static void ShowKeysTip(IEnumerable<(string key, IEnumerable<string> descriptions)> tips)
        {
            if (tips == null) return;
            var description = tips.SelectMany(t => t.descriptions.Select(d => new TipItem(){key=t.key, description= d}));
            var b = new FancyBalloon(){Tips = new ObservableCollection<TipItem>(description)};
            // var formattedText = new FormattedText(
            //     description,
            //     CultureInfo.CurrentCulture,
            //     FlowDirection.LeftToRight,
            //     new Typeface(b.TextBlock.FontFamily, b.TextBlock.FontStyle, b.TextBlock.FontWeight, b.TextBlock.FontStretch),
            //     b.TextBlock.FontSize,
            //     Brushes.Black,
            //     new NumberSubstitution(),
            //     1);
            // b.Width = formattedText.Width + 8;
            // b.Height = formattedText.Height + 8;

            trayIcon.ShowCustomBalloon(b,PopupAnimation.None,8000);
        }

    }
}