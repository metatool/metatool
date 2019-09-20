```cs
private readonly WeakEventSource<MyEventArgs> _myEventSource = new WeakEventSource<MyEventArgs>();
public event EventHandler<MyEventArgs> MyEvent
{
    add { _myEventSource.Subscribe(value); }
    remove { _myEventSource.Unsubscribe(value); }
}
 _myEventSource.Invoke(this, e);
```