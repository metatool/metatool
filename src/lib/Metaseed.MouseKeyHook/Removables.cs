using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metaseed.Input
{
    public interface IRemovable
    {
        void Remove();
    }


    public class Removable : IRemovable
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

    public class Removables :  List<IRemovable>,IRemovable
    {
        public void Remove()
        {
            this.ForEach(d=>d.Remove());
        }
    }
}
