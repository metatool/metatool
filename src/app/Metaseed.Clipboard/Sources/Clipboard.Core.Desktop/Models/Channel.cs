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
        internal       string                       Name;
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
            _channels.Values.ToList().ForEach(c=>c.Clear());
            _channels.Clear();
        }

        public static void Remove(DataEntry entry)
        {
            if(entry.RegisterLocation == "") return;
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
        readonly Dictionary<int, DataEntry> _buffer = new Dictionary<int, DataEntry>();

        public void Set(DataEntry data, int index = -1)
        {
            if (Content.Contains(data)|| _buffer.ContainsValue(data)) return;

            if (index < Content.Count && index != -1)
            {
                Console.WriteLine("Warning: duplicate entry found! This entry would not be added.");
                data.RegisterLocation = "";
                return;
            }
            var i = index == -1 ? 0 : index;

            if (i > Content.Count)
            {
                _buffer.Add(i, data);
            }
            else
            {
               
                Content.Insert(i, data);
                
                foreach (var key in _buffer.Keys)
                {
                    if (key == Content.Count)
                    {
                        Content.Insert(key, _buffer[key]);
                        _buffer.Remove(key);
                    }
                    else if (i < Content.Count)
                    {
                        Console.WriteLine("Warning: duplicate entry found! This entry would not be added.");
                        _buffer[key].RegisterLocation = "";
                        return;
                    }
                }
            }

            data.RegisterLocation = Name + "." + i;
        }

        private void Clear()
        {
            foreach (var dataEntry in Content)
            {
                dataEntry.RegisterLocation = "";
            }

            Content.Clear();
        }

        private void RemoveItem(DataEntry entry)
        {
            Content.Remove(entry);

        }
    }
}