using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Clipboard.Core.Desktop.ComponentModel;
using Clipboard.Core.Desktop.Enums;
using Clipboard.Core.Desktop.Models;
using Clipboard.Strings;
using GalaSoft.MvvmLight;

namespace Clipboard.ViewModels
{
    internal class CopyTipsViewModel : ViewModelBase
    {
        public  ListCollectionView CollectionView { get; private set; }
        private bool               _isReplaceAll;

        private readonly ObservableCollection<DataEntry> _replaceAllDataEntries;
        private          ObservableCollection<DataEntry> _allDataEntries;


        public CopyTipsViewModel()
        {
            var entry = new DataEntry() {Thumbnail = new Thumbnail() {Type = ThumbnailDataType.New}};
            _replaceAllDataEntries = new ObservableCollection<DataEntry>() {entry};
        }

        internal void SetData(ObservableCollection<DataEntry> value)
        {
            _allDataEntries = new ObservableCollection<DataEntry>(value);
            _allDataEntries.Insert(0, _replaceAllDataEntries[0]);
            CollectionView = (ListCollectionView) CollectionViewSource.GetDefaultView(_allDataEntries);
            if (_allDataEntries.Count > 0) CollectionView.MoveCurrentToPosition(0);
            RaisePropertyChanged(nameof(CollectionView));
        }


        private Channel _channel = null;

        internal void SwitchWithNext(int index)
        {
            if (IsReplaceAll) return;
            var entry = _allDataEntries?[index];
            _allDataEntries?.RemoveAt(index);
            _allDataEntries?.Insert(index + 1, entry);
            _channel.CurrentIndex = index + 1;
        }

        internal void SwitchWithLast(int index)
        {
            SwitchWithNext(index - 1);
        }

        public bool IsReplaceAll
        {
            get => _isReplaceAll;
        }

        internal void SetChannelData(Channel channel)
        {
            var lastChannel = _channel;
            _channel = channel;
            if (channel == lastChannel)
                ToggleChannelIsReplaceAll(channel);
            else
                SetData(channel.GetContent());
        }

        private int indexBackup = -1;

        private void ToggleChannelIsReplaceAll(Channel channel)
        {
            _isReplaceAll ^= true;

            if (_isReplaceAll)
            {
                channel.CurrentIndex = -1; // replace all
                indexBackup          = CollectionView.CurrentPosition;
                CollectionView       = (ListCollectionView) CollectionViewSource.GetDefaultView(_replaceAllDataEntries);
            }
            else
            {
                CollectionView = (ListCollectionView) CollectionViewSource.GetDefaultView(_allDataEntries);
                CollectionView.MoveCurrentToPosition(indexBackup);
                channel.CurrentIndex = indexBackup;
            }

            RaisePropertyChanged(nameof(CollectionView));

            RaisePropertyChanged(nameof(IsReplaceAll));
        }

        internal void ResetIsReplaceAll()
        {
            _isReplaceAll = false;
            indexBackup   = -1;
            if (_channel != null)
            {
                _channel.CurrentIndex = -1;
                _channel              = null;
            }

            RaisePropertyChanged(nameof(IsReplaceAll));
        }

        public LanguageManager Language => LanguageManager.GetInstance();
    }
}