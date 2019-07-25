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
        private Channel _channel;

        private const string PasteAllTips = "←Paste all from right to left←";
        private const string PasteCurrent = "Use 'C', 'V' to change selection";
        internal void SetData(ObservableCollection<DataEntry> value)
        {
            if (_dataEntries == value) return;
            _dataEntries   = value;
            CollectionView = (ListCollectionView) CollectionViewSource.GetDefaultView(_dataEntries);
            if (_dataEntries.Count > 0) CollectionView.MoveCurrentToPosition(0);
            RaisePropertyChanged(nameof(CollectionView));
        }

        public Channel Channel
        {
            get => _channel;
            set
            {
                if (value == _channel) return;
                _channel = value;
                RaisePropertyChanged(nameof(Channel));
            }
        }

        private string _tips = PasteCurrent;
        public string Tips
        {
            get => _tips;
            set
            {
                if (value == _tips) return;
                _tips = value;
                RaisePropertyChanged(nameof(Tips));
            }
        }

        public bool IsPasteAll
        {
            get => _isPasteAll;
        }

        internal void SetChannelData(Channel channel)
        {
            SetData(channel.GetContent());
            var lastChannel = Channel;
            Channel = channel;
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
                Tips = PasteAllTips;
                Channel.CurrentIndex = this.CollectionView.CurrentPosition;
                this.CollectionView.MoveCurrentTo(null);
            }
            else
            {
                Tips = PasteCurrent;
                this.CollectionView.MoveCurrentToPosition(Channel.CurrentIndex);
            }

            RaisePropertyChanged(nameof(IsPasteAll));
        }


        public LanguageManager Language => LanguageManager.GetInstance();
    }
}