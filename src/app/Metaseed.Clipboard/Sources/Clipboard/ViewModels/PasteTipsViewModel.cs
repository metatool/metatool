using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Clipboard.Core.Desktop.ComponentModel;
using Clipboard.Core.Desktop.Models;
using GalaSoft.MvvmLight;

namespace Clipboard.ViewModels
{
    public class PasteTipsViewModel: ViewModelBase
    {
        private AsyncObservableCollection<DataEntry> _dataEntries;
        internal AsyncObservableCollection<DataEntry> DataEntries
        {
            get => _dataEntries;
            set
            {
                _dataEntries                   = value;
                RaisePropertyChanged(nameof(DataEntries));
            }
        }
    }
}
