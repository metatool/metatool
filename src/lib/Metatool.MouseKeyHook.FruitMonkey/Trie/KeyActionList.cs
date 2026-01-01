using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

internal class KeyActionList<TValue> : ObservableCollection<TValue> where TValue : KeyEventCommand
{
    public IEnumerable<KeyCommand> this[KeyEventType keyEventType]
    {
        get
        {
            switch (keyEventType)
            {
                case KeyEventType.Down:
                    return Down;
                case KeyEventType.Up:
                    return Up;
                case KeyEventType.AllUp:
                    return AllUp;
                default:
                    throw new Exception(keyEventType + "not supported.");
            }
        }
    }

    private bool _refresh = true;
    private IEnumerable<KeyCommand>? _down;

    public IEnumerable<KeyCommand> Down
    {
        get
        {
            if (_down != null && !_refresh) return _down;

            _down = this.Where(e => e.KeyEventType == KeyEventType.Down).Select(e => e.Command);
            _refresh = false;
            return _down;
        }
    }

    private IEnumerable<KeyCommand>? _up;

    public IEnumerable<KeyCommand> Up
    {
        get
        {
            if (_up != null && !_refresh) return _up;

            _up = this.Where(e => e.KeyEventType == KeyEventType.Up).Select(e => e.Command);
            _refresh = false;
            return _up;
        }
    }

    private IEnumerable<KeyCommand>? _allUp;

    public IEnumerable<KeyCommand> AllUp
    {
        get
        {
            if (_allUp != null && !_refresh) return _allUp;

            _allUp = this.Where(e => e.KeyEventType == KeyEventType.AllUp).Select(e => e.Command);
            _refresh = false;
            return _allUp;
        }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        _refresh = true;
    }
}