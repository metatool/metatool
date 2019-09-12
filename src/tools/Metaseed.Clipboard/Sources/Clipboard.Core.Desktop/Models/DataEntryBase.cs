using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Clipboard.Core.Desktop.Models
{
    /// <summary>
    /// Represents the basic information of a data entry locally or on a cloud server.
    /// </summary>
    [Serializable]
    internal class DataEntryBase: INotifyPropertyChanged
    {
        private bool _isFavorite;

        /// <summary>
        /// Gets or sets the data entry identifier
        /// </summary>
        [JsonProperty]
        internal Guid Identifier { get; set; }

        /// <summary>
        /// Gets or sets the list of identifiers for a clipboard data
        /// </summary>
        [JsonProperty]
        internal List<DataIdentifier> DataIdentifiers { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> that defines when the data has been copied.
        /// </summary>
        [JsonProperty]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets a value that defines whether this data is a favorite or not.
        /// </summary>
        [JsonProperty]
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if(value == _isFavorite) return;
                _isFavorite = value;
                OnPropertyChanged(nameof(IsFavorite));
            }
        }

        #region Methods

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}

