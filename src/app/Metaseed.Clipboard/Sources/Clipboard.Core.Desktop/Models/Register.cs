using System;
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

        private        string         _name;
        private static Dictionary<string,Register> _registers = new Dictionary<string, Register>();

        private Register(string name)
        {
            _name = name;
        }

        private static Register CreateRegister(string name)
        {
            var r =  new Register(name);
            _registers.Add(name,r);
            return r;
        }

        public static Register GetRegister(string name)
        {
            if (_registers.TryGetValue(name, out var r)) return r;
            return CreateRegister(name);
        }

        public bool? IsAppend;

        private ObservableCollection<DataEntry> Content { get; set; } = new AsyncObservableCollection<DataEntry>();

        public void Set(DataEntry data)
        {
            if(!IsAppend.GetValueOrDefault()) Content.Clear();
            Content.Add(data);

        }

        public ObservableCollection<DataEntry> GetContent()
        {
            return Content;
        }
    }
}