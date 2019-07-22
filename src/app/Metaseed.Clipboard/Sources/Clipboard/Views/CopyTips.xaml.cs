using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Clipboard.Core.Desktop.Models;
using Clipboard.Shared.Logs;
using Clipboard.ViewModels;

namespace Clipboard.Views
{
    /// <summary>
    /// Interaction logic for PasteBarList.xaml
    /// </summary>
    public partial class CopyTips : UserControl
    {
        public CopyTips()
        {
            InitializeComponent();
            this.Loaded+=(o,e) =>
            ViewModel.CollectionView.MoveCurrentToFirst();
        }
        private void ScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
        }
        internal CopyTipsViewModel ViewModel => (CopyTipsViewModel) DataContext;



        internal void Next()
        {
            var view     = ViewModel.CollectionView;
            var position = view.CurrentPosition;
            if(position >= view.Count-1) return;
            ViewModel.SwitchWithNext(position);
            var r        = view.MoveCurrentToPosition(position +1);
            if (r) DataListBox.ScrollIntoView(view.CurrentItem);
        }

        internal void Previous()
        {
            var view = ViewModel.CollectionView;
            var position =view.CurrentPosition;
            if(position <= 0) return;
            ViewModel.SwitchWithLast(position);
            var r = view.MoveCurrentToPosition(position -1);
            if(r) DataListBox.ScrollIntoView(view.CurrentItem);
        }

        internal int CurrentItemIndex => ViewModel.CollectionView.CurrentPosition;
    }
}
