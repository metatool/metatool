using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Metatool.UI
{
    public class MessageToken<T>
    {
        private readonly Popup           _popup;
        internal         DispatcherTimer Timer;
        public           bool            IsClosed;

        public MessageToken(Popup popup)
        {
            _popup = popup;
        }

        public void Close()
        {
            if (IsClosed) return;
            var dataContext = _popup.DataContext as ObservableCollection<T>;
            dataContext.Clear();
            _popup.IsOpen = false;
            Timer?.Stop();
            IsClosed = true;
        }

        public void Refresh()
        {
            IsClosed      = false;
            _popup.IsOpen = true;
            Timer?.Stop();
            Timer?.Start();
        }
    }
}
