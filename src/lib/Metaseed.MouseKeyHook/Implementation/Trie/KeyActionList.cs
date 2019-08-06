using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Metaseed.Input.MouseKeyHook.Implementation
{
    internal class KeyActionList<TValue>: BindingList<TValue> where TValue: KeyEventAction
    {
        public IEnumerable<KeyCommand> this[KeyEvent keyEvent]
        {
            get
            {
                switch (keyEvent)
                {
                    case KeyEvent.Down:
                        return Down;
                    case KeyEvent.Up:
                        return Up;
                    default:
                        throw  new Exception(keyEvent + "not supported.");
                }
            }
        }

        private bool _refresh = true;
        private IEnumerable<KeyCommand> _down;
        public IEnumerable<KeyCommand> Down 
        {
            get
            {
                if (_down != null && !_refresh) return _down;
                _down    = this.Where(e => e.KeyEvent == KeyEvent.Down).Select(e=>e.Command);
                _refresh = false;
                return _down;
            }
        }

        private IEnumerable<KeyCommand> _up;
        public IEnumerable<KeyCommand> Up
        {
            get
            {
                if (_up != null && !_refresh) return _up;
                _up    = this.Where(e => e.KeyEvent == KeyEvent.Up).Select(e=>e.Command);
                _refresh = false;
                return _up;

            }
        }


        protected override void OnListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);
            _refresh = true;
        }
    }
}
