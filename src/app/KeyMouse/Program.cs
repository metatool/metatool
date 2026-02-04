using System;
using System.Windows;

namespace KeyMouse
{
    /// <summary>
    /// Program entry point for the KeyMouse WPF application
    /// </summary>
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fatal error: {ex.Message}\n{ex.StackTrace}", "KeyMouse Error");
                Environment.Exit(1);
            }
        }
    }
}
