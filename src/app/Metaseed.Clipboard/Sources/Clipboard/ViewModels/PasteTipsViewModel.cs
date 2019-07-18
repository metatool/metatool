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

        public ObservableCollection<DataEntry> DataEntries
        {
            get => _dataEntries;
            set
            {
                _dataEntries   = value;
                CollectionView = (ListCollectionView) CollectionViewSource.GetDefaultView(_dataEntries);
                if (_dataEntries.Count > 0) CollectionView.MoveCurrentToPosition(0);
                RaisePropertyChanged(nameof(CollectionView));
            }
        }
        internal Channel Channel = null;

        public bool IsPasteAll
        {
            get => _isPasteAll;
        }

        internal void ChangeIsPasteAllState(Channel channel)
        {
            var lastChannel = Channel;
            Channel = channel;
            if (channel == lastChannel)
                ToggleChannelIsPasteAll();
            else
                ResetIsPasteAll();
        }
        internal void ToggleChannelIsPasteAll()
        {
            _isPasteAll ^= true;
            if (_isPasteAll)
                this.CollectionView.MoveCurrentTo(null);
            else
            {
                this.CollectionView.MoveCurrentToLast();
            }

            RaisePropertyChanged(nameof(IsPasteAll));
        }

        internal void ResetIsPasteAll()
        {
            if (!_isPasteAll) return;
            _isPasteAll = false;
            RaisePropertyChanged(nameof(IsPasteAll));

        }
        public LanguageManager Language => LanguageManager.GetInstance();
    }
}