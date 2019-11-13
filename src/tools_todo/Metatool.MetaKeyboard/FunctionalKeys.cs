using Metatool.Command;
using Metatool.Service;
using static Metatool.Service.Key;

namespace Metatool.MetaKeyboard
{
    public class FunctionalKeys : CommandPackage
    {
        public FunctionalKeys()
        {
            RegisterCommands();
        }
        public IKeyCommand  CloseMetaKeysCommand = (LWin + L).Handled().Down(null);


        public IKeyCommand  CloseMetaKeyCommand = (LCtrl + LWin + LAlt +  C)
            .Down(e =>
            {
                var notify = Services.Get<INotify>();
                notify.ShowMessage("MetaKeyBoard Closing...");
                Context.Exit(0);
            }, null, "Close");

        public IKeyCommand  RestartMetakeyAdmin = (LCtrl + LWin + LAlt + X).Down(e =>
        {
            var notify = Services.Get<INotify>();
            notify.ShowMessage("MetaKeyBoard Restarting...");
            Context.Restart(0, true);
        }, null, "Restart(Admin)");

        public IKeyCommand  ShowTips = (Caps + Question).Down(e =>
        {
            var keyboard = Services.Get<IKeyboard>();

            //Keyboard.Default.ShowTip();
            e.Handled = true;
        }, null, "Show Tips");

      
    }
}
