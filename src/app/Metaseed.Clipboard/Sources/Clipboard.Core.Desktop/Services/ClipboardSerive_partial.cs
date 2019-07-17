using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Clipboard.Core.Desktop.Models;
using Clipboard.Shared.Services;
namespace Clipboard.Core.Desktop.Services
{
    internal partial class ClipboardService
    {
        private Register _register;

        internal void CopyTo(Register register)
        {
            _register = register;
            Metaseed.Input.Keyboard.Hit(Keys.C, new List<Keys> {Keys.RControlKey});
        }

        DataService dataService;

        internal DataService DataService
        {
            get =>
                dataService = dataService ?? ServiceLocator.GetService<DataService>();
        }

        internal void PasteFrom(int index)
        {
            if (DataService.DataEntries.Count < index)
            {
                Console.WriteLine("no data in DataEntries");
                return;
            }

            var data = DataService.DataEntries[index];

            DataService.CopyData(data);
            this.Paste(50);
        }


        internal void PasteFrom(Register register)
        {
            if (register == null)
            {
                this.Paste(50);
                return;
            }

            register.GetContent().ToList().ForEach(data =>
            {
                DataService.CopyData(data);
                this.Paste(50);
            });
        }

        private void AddTo(DataEntry data)
        {
            if (_register == null) return;
            _register.Set(data);
            _register.IsAppend = null;
            _register = null;
        }

    }
}