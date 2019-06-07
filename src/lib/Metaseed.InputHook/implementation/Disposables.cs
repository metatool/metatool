using System;
using System.Collections.Generic;
using System.Text;

namespace Metaseed.Input.implementation
{
    public class Disposables :  List<IDisposable>,IDisposable
    {
        public void Dispose()
        {
            this.ForEach(d=>d.Dispose());
        }
    }
}
