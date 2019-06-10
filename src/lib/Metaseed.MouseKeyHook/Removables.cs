using System;
using System.Collections.Generic;
using System.Text;

namespace Metaseed.Input
{
    public interface IRemovable
    {
        void Remove();
    }
    public class Removables :  List<IRemovable>,IRemovable
    {
        public void Remove()
        {
            this.ForEach(d=>d.Remove());
        }
    }
}
