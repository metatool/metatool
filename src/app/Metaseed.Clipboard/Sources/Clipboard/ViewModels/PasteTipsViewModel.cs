using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Clipboard.Core.Desktop.ComponentModel;
using Clipboard.Core.Desktop.Models;
using Clipboard.Strings;
using GalaSoft.MvvmLight;

namespace Clipboard.ViewModels
{
    internal class PasteTipsViewModel : ViewModelBase
    {
        public  ListCollectionView              CollectionView { get; private set; }
        private ObservableCollection<DataEntry> _dataEntries;
        private bool                            _isPasteAll;

        internal void SetData(ObservableCollection<DataEntry> value)
        {
            if (_dataEntries == value) return;
            _dataEntries   = value;
            CollectionView = (ListCollectionView) CollectionViewSource.GetDefaultView(_dataEntries);
            if (_dataEntries.Count > 0) CollectionView.MoveCurrentToPosition(0);
            RaisePropertyChanged(nameof(CollectionView));
        }

        private Channel _channel = null;

        public bool IsPasteAll
        {
            get => _isPasteAll;
        }

        internal void SetChannelData(Channel channel)
        {
            SetData(channel.GetContent());
            var lastChannel = _channel;
            _channel = channel;
            if (channel == lastChannel)
            {
                ToggleChannelIsPasteAll();
            }
        }

        private void ToggleChannelIsPasteAll()
        {
            _isPasteAll ^= true;
            if (_isPasteAll)
            {
                _channel.CurrentIndex = this.CollectionView.CurrentPosition;
                this.CollectionView.MoveCurrentTo(null);
            }
            else
            {
                this.CollectionView.MoveCurrentToPosition(_channel.CurrentIndex);
            }

            RaisePropertyChanged(nameof(IsPasteAll));
        }

        internal void ResetIsPasteAll()
        { 
            _isPasteAll           = false;
            if (_channel != null)
            {
                _channel.CurrentIndex = -1;
                _channel              = null;
            }

            RaisePropertyChanged(nameof(IsPasteAll));
        }

        public LanguageManager Language => LanguageManager.GetInstance();
    }
}