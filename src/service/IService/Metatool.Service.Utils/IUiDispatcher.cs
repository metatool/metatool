using System;
using System.Threading.Tasks;

namespace Metatool.Service;

public interface IUiDispatcher
{
    void Dispatch(Action action);
    Task DispatchAsync(Func<Task> action);
    bool CheckAccess();
}