using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clipboard.Core.Desktop.ComponentModel;

namespace Clipboard.Core.Desktop.Models
{
    internal class Channel
    {
        internal       string                      Name;
        private static Dictionary<string, Channel> _channels = new Dictionary<string, Channel>();

        private Channel(string name)
        {
            Name = name;
        }

        private static Channel CreateRegister(string name)
        {
            var r = new Channel(name);
            _channels.Add(name, r);
            return r;
        }

        public static void ClearAll()
        {
            _channels.Values.ToList().ForEach(c => c.Clear());
            _channels.Clear();
        }

        public static void Remove(DataEntry entry)
        {
            if (entry.RegisterLocation == "") return;
            _channels[entry.RegisterLocation].RemoveItem(entry);
        }

        public static Channel GetRegister(string name)
        {
            if (_channels.TryGetValue(name, out var r)) return r;
            return CreateRegister(name);
        }

        private ObservableCollection<DataEntry> Content { get; set; } = new ObservableCollection<DataEntry>();

        internal ObservableCollection<DataEntry> GetContent()
        {
            return Content;
        }


        public void Set(DataEntry data, bool isAppend = false)
        {
            if (Content.Contains(data) ) return;

            if (isAppend)
            {
                Content.Add(data);
            }
            else
            {
                data.ChannelName       = Name;
                data.SequenceInChannel = Content.Count;
                Content.Insert(0, data);
            }
        }

        private void Clear()
        {
            foreach (var dataEntry in Content)
            {
                dataEntry.ChannelName = null;
                dataEntry.SequenceInChannel = -1;
            }

            Content.Clear();
        }

        private void RemoveItem(DataEntry entry)
        {
            Content.Remove(entry);
        }
    }
}