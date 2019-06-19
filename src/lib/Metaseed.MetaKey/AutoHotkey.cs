using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Forms;

namespace Metaseed.MetaKey
{

    public sealed class AutoHotkey :  IDisposable 
    {
        private bool disposed;
        
        [Import(RequiredCreationPolicy = CreationPolicy.NonShared)]
        public IKeyboard Keyboard { get; set; }

        [Import]
        public IProcesses Processes { get; set; }

        [Import]
        public IRegistry Registry { get; set; }

        public AutoHotkey()
        {
            //Console.TreatControlCAsInput = true;
            //Trace.Listeners.Add(new ConsoleTraceListener());
            var threadId = Helpers.GetCurrentThreadId();
            AppDomain.CurrentDomain.DomainUnload += delegate
            {
                Helpers.TraceResult(Helpers.PostThreadMessage((uint)threadId, 0, 0, 0), "PostThreadMessage");
            };
        }


        void IDisposable.Dispose()
        {
            Trace.WriteLine("DISPOSE");
            if(disposed)
            {
                return;
            }
            disposed = true;
            Keyboard.Dispose();
        }

        public void Run()
        {
            using(this)
            {
                Application.Run();
            }
        }

    }
}