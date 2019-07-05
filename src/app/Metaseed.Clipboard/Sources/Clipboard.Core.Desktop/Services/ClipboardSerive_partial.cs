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

        internal void PasteFrom(Register register)
        {
            var dataService = ServiceLocator.GetService<DataService>();
            register?.GetContent().ToList().ForEach(data =>
            {
                dataService.CopyData(data);
                this.Paste();
            });
        }

        private void AddTo(DataEntry data)
        {
            if (_register != null)
            {
                _register.Add(data);
                _register = null;
            }
        }
    }
}