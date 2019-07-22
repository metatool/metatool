using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;
using Clipboard.Core.Desktop.ComponentModel;

namespace Clipboard.Core.Desktop.Models
{
    internal class Channel
    {
        internal       string                      Name;
        private static readonly Dictionary<string, Channel> Channels = new Dictionary<string, Channel>();

        private Channel(string name)
        {
            Name = name;
        }

        private static Channel CreateChannel(string name)
        {
            var r = new Channel(name);
            Channels.Add(name, r);
            return r;
        }

        public static void ClearAll()
        {
            Channels.Values.ToList().ForEach(c => c.Clear());
            Channels.Clear();
        }

        public static void Remove(DataEntry entry)
        {
            if (entry.RegisterLocation == "") return;
            Channels[entry.RegisterLocation].RemoveItem(entry);
        }

        public static Channel GetChannel(string name)
        {
            if (Channels.TryGetValue(name, out var r)) return r;
            return CreateChannel(name);
        }

        private ObservableCollection<DataEntry> _entries { get; set; } = new ObservableCollection<DataEntry>();

        internal ObservableCollection<DataEntry> GetContent()
        {
            return _entries;
        }

        internal int CurrentIndex =-1;

        public void Set(DataEntry data, int index = -1)
        {
            if (_entries.Contains(data) ) return;

            if (index == -1)
            {
                _entries.Add(data);
            }
            else
            {
                data.ChannelName       = Name;
                data.SequenceInChannel = _entries.Count;
                _entries.Insert(index, data);
            }
        }

        private void Clear()
        {
            foreach (var dataEntry in _entries)
            {
                dataEntry.ChannelName = null;
                dataEntry.SequenceInChannel = -1;
            }

            _entries.Clear();
        }

        private void RemoveItem(DataEntry entry)
        {
            _entries.Remove(entry);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}