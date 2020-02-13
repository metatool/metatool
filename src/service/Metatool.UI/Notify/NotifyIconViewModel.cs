using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Metatool.Utils.Notify
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public class NotifyIconViewModel
    {
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand<MenuItem>
                {
                    CommandAction = ( m) =>
                    {
                        if (m == null)
                        {
                            Application.Current.MainWindow.Show();
                            return;
                        }

                        if (Application.Current.MainWindow.IsVisible)
                        {
                            m.Header = "Show Window";
                            Application.Current.MainWindow.Hide();
                        }
                        else
                        {
                            m.Header = "Hide Window";
                            Application.Current.MainWindow.Show();
                        }
                    }
                };
            }
        }


        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand<object> {CommandAction = o => Application.Current.Shutdown()};
            }
        }
    }


    /// <summary>
    /// Simplistic delegate command for the demo.
    /// </summary>
    public class DelegateCommand<T> : ICommand where T:class
    {
        public Action<T> CommandAction { get; set; }
        public Func<T,bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction(parameter as T);
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null  || CanExecuteFunc(parameter as T);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
