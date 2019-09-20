using System;
using System.Collections.Generic;
using OneOf;

namespace Metatool.Input
{
    using Hotkey = OneOf<ISequenceUnit, ISequence>;

    public interface IRemove
    {
        void Remove();
    }

    public interface IRemoveChangeable<T> : IRemove
    {
        bool Change(T key);
    }

    public interface IRemoveChangeableKey: IRemoveChangeable<Hotkey>
    {
    }

    public class Removable : IRemove
    {
        private readonly Action _removeAction;

        public Removable(Action removeAction)
        {
            _removeAction = removeAction;
        }

        public void Remove()
        {
            _removeAction();
        }
    }

    public class Removables : List<IRemove>, IRemove
    {
        public void Remove()
        {
            ForEach(d => d.Remove());
        }
    }

    public class RemoveChangeables<T> : List<IRemoveChangeable<T>>, IRemoveChangeable<T>
    {
        public void Remove()
        {
            ForEach(d => d.Remove());
        }

        public bool Change(T key)
        {
            ForEach(d => d.Change(key));
            return true;
        }
    }
}
