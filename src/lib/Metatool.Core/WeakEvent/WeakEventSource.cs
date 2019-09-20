using System;
using static Metatool.Core.WeakEvent.WeakEventSourceHelper;

namespace Metatool.Core.WeakEvent
{

    public class WeakEventSource<TEventArgs>
    {
        private DelegateCollection _handlers;

        public void Invoke(object sender, TEventArgs e)
        {
            var validHandlers = GetValidHandlers(_handlers);
            foreach (var handler in validHandlers)
            {
                handler.Invoke(sender, e);
            }
        }

        public void Subscribe(EventHandler<TEventArgs> handler)
        {
            Subscribe(null, handler);
        }

        public void Subscribe(object lifetimeObject, EventHandler<TEventArgs> handler)
        {
            Subscribe<DelegateCollection, OpenEventHandler, StrongHandler>(lifetimeObject, ref _handlers, handler);
        }

        public void Unsubscribe(EventHandler<TEventArgs> handler)
        {
            Unsubscribe(null, handler);
        }

        public void Unsubscribe(object lifetimeObject, EventHandler<TEventArgs> handler)
        {
            Unsubscribe<OpenEventHandler, StrongHandler>(lifetimeObject, _handlers, handler);
        }

        private delegate void OpenEventHandler(object target, object sender, TEventArgs e);

        private struct StrongHandler
        {
            private readonly object _target;
            private readonly OpenEventHandler _openHandler;

            public StrongHandler(object target, OpenEventHandler openHandler)
            {
                _target = target;
                _openHandler = openHandler;
            }

            public void Invoke(object sender, TEventArgs e)
            {
                _openHandler(_target, sender, e);
            }
        }

        private class DelegateCollection : DelegateCollectionBase<OpenEventHandler, StrongHandler>
        {
            public DelegateCollection()
                : base((target, openHandler) => new StrongHandler(target, openHandler))
            {
            }
        }
    }
}