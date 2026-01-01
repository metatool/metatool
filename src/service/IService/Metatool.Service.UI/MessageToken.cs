using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Metatool.UI;

public class MessageToken<T>
{
    private readonly DispatcherOperation _operation;
    private Popup _popup;
    internal DispatcherTimer Timer;
    public bool IsClosed;

    public MessageToken(Popup popup)
    {
        _popup = popup;
    }

    public MessageToken(DispatcherOperation operation)
    {
        _operation = operation;
    }

    public void Close()
    {
        if (IsClosed) return;
        GetPopup();
        var dataContext = _popup.DataContext as ObservableCollection<T>;
        dataContext.Clear();
        _popup.IsOpen = false;
        Timer?.Stop();
        IsClosed = true;
    }

    private void GetPopup()
    {
        if (_popup == null)
        {
            _operation.Wait();
            _popup = (_operation.Result as MessageToken<T>)!._popup;
        }
    }

    public void Refresh()
    {
        IsClosed = false;
        GetPopup();
        _popup.IsOpen = true;
        Timer?.Stop();
        Timer?.Start();
    }
}