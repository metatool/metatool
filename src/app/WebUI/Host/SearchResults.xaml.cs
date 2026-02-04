using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Metatool.WebViewHost
{
    public partial class SearchResults : Window
    {
        public class Item
        {
            public string FilePath { get; set; }
            public int LineNumber { get; set; }
            public string LinePreview { get; set; }
        }

        public SearchResults(string query, List<Item> items)
        {
            InitializeComponent();
            Header.Text = $"Search: '{query}' — {items.Count} result(s)";
            ResultsList.ItemsSource = items;
        }

        private void ResultsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ResultsList.SelectedItem is Item it && !string.IsNullOrEmpty(it.FilePath))
            {
                try
                {
                    var psi = new ProcessStartInfo(it.FilePath) { UseShellExecute = true };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Failed to open file: " + ex.Message, "Open File", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
