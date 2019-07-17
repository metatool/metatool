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
    internal class PasteTipsViewModel: ViewModelBase
    {
        private ObservableCollection<DataEntry> _collectionView;
        public ObservableCollection<DataEntry> CollectionView
        {
            get => _collectionView;
            set
            {
                _collectionView                   = value;
                RaisePropertyChanged(nameof(CollectionView));
            }
        }

        public LanguageManager Language => LanguageManager.GetInstance();
    }
}
