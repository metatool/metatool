using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Clipboard.Core.Desktop.Models;
using Clipboard.Shared.Services;
namespace Clipboard.Core.Desktop.Services
{
    internal partial class ClipboardService
    {
        private Channel _channel;

        internal void CopyTo(Channel channel)
        {
            _channel = channel;
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
            this.Paste();
        }


        internal void PasteFrom(Channel channel, int index)
        {
            if (channel == null)
            {
                this.Paste();
                return;
            }

            if (index != -1)
            {
                DataService.CopyData(channel.GetContent()[index]);
                this.Paste();
                return;
            }

            channel.GetContent().ToList().ForEach(data =>
            {
                DataService.CopyData(data);
                Thread.Sleep(100);
                this.Paste();
                Thread.Sleep(100);
            });
        }

        private void AddTo(DataEntry data)
        {
            if (_channel == null) return;
            _channel.Set(data);
            _channel = null;
        }

    }
}