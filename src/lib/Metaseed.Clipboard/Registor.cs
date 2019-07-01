using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Cp= Windows.ApplicationModel.DataTransfer.Clipboard;
namespace Metaseed.Clipboard
{
    public class Registor
    {
        private string               _RegistorContentId;
        private ClipboardHistoryItem _Registor;
        public ClipboardHistoryItem Content
        {
            get
            {
                if (_Registor != null) return _Registor;
                if (_RegistorContentId == null) return null;
                var items = Cp.GetHistoryItemsAsync().GetAwaiter().GetResult().Items;
                var item = items.FirstOrDefault(it => it.Id == _RegistorContentId);
                if (item == null)
                {
                    _RegistorContentId = null;
                    return null;
                }
                _Registor = item;
                return _Registor;
            }
            set
            {
                _Registor = value;
                _RegistorContentId = value.Id;
            }
        }
    }
}
