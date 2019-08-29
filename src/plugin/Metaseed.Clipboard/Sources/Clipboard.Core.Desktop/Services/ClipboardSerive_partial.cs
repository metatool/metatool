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

        DataService _dataService;

        internal DataService DataService => _dataService = _dataService ?? ServiceLocator.GetService<DataService>();

        internal async Task PasteFrom(int index)
        {
            if (DataService.DataEntries.Count < index)
            {
                Console.WriteLine("no data in DataEntries");
                return;
            }

            var data = DataService.DataEntries[index];

            DataService.CopyData(data);
            await PasteAsync();
        }


        internal async Task PasteFrom(Channel channel, int index)
        {
            if (channel == null)
            {
                await PasteAsync();
                return;
            }

            if (index != -1)
            {
                DataService.CopyData(channel.GetContent()[index]);
                await PasteAsync();
                return;
            }

            foreach (var data in channel.GetContent().Reverse())
            {
                DataService.CopyData(data);
                await PasteAsync(200);
            };
        }

        private void AddTo(DataEntry data)
        {
            if (_channel == null) return;
            if (_channel.CurrentIndex == -1)
            {
                _channel.Clear();
                _channel.Set(data);
            }
            else
            {
                _channel.Set(data, _channel.CurrentIndex);
            }

            _channel = null;
        }

    }
}