using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Metatool.Input.MouseKeyHook.Implementation
{
    internal class KeyActionList<TValue>: BindingList<TValue> where TValue: KeyEventCommand
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
                    case KeyEvent.AllUp:
                        return AllUp;
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
        private IEnumerable<KeyCommand> _allUp;
        public IEnumerable<KeyCommand> AllUp
        {
            get
            {
                if (_allUp != null && !_refresh) return _allUp;
                _allUp = this.Where(e => e.KeyEvent == KeyEvent.AllUp).Select(e => e.Command);
                _refresh = false;
                return _allUp;

            }
        }

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);
            _refresh = true;
        }
    }
}
