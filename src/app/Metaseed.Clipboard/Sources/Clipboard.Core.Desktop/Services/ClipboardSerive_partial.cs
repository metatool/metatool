using System;
using System.Collections.Generic;
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

        DataService DataService
        {
            get =>
                dataService = dataService ?? ServiceLocator.GetService<DataService>();
        }

        internal void PasteFrom(int relevantToCurrentIndex)
        {
            var index = DataService.CurrentIndex + relevantToCurrentIndex;
            if (DataService.DataEntries.Count < index)
            {
                Console.WriteLine("no data in DataEntries");
                return;
            }

            DataService.CurrentIndex = index;

            var data = DataService.DataEntries[index];

            DataService.CopyData(data);
            this.Paste(false);
        }

        internal void PasteFrom(Register register)
        {
            if (register == null)
            {
                this.Paste(false);
                return;
            }

            register.GetContent().ToList().ForEach(data =>
            {
                DataService.CopyData(data);
                this.Paste(false);
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