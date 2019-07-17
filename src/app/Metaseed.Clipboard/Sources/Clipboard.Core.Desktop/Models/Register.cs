﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clipboard.Core.Desktop.ComponentModel;

namespace Clipboard.Core.Desktop.Models
{
    internal class Register
    {
        internal       string                       Name;
        private static Dictionary<string, Register> _registers = new Dictionary<string, Register>();

        private Register(string name)
        {
            Name = name;
        }

        private static Register CreateRegister(string name)
        {
            var r = new Register(name);
            _registers.Add(name, r);
            return r;
        }

        public static Register GetRegister(string name)
        {
            if (_registers.TryGetValue(name, out var r)) return r;
            return CreateRegister(name);
        }

        internal bool? IsAppend;

        private ObservableCollection<DataEntry> Content { get; set; } = new AsyncObservableCollection<DataEntry>();

        internal ObservableCollection<DataEntry> GetContent()
        {
            return Content;
        }
        readonly Dictionary<int, DataEntry> _buffer = new Dictionary<int, DataEntry>();

        public void Set(DataEntry data, int index = -1)
        {
            if (Content.Contains(data)|| _buffer.ContainsValue(data)) return;

            if (!IsAppend.GetValueOrDefault())
            {
                foreach (var dataEntry in Content)
                {
                    dataEntry.RegisterLocation = "";
                }

                Content.Clear();
            }
            var i = index == -1 ? Content.Count : index;
            if (i < Content.Count)
            {
                Console.WriteLine("Warning: duplicate entry found! This entry would not be added.");
                data.RegisterLocation = "";
                return;
            }

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
    }
}