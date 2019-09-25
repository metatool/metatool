using System;
using System.Collections.Generic;
using System.Text;

namespace Metatool.Command
{
    public interface ICommandToken<in T> : IChangeRemove<T>
    {

    }
}
