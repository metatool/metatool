using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;
using Metatool.Input;
using Microsoft.Win32;
using Cp = Windows.ApplicationModel.DataTransfer.Clipboard;
using ListView = System.Windows.Forms.ListView;

namespace Metatool.Clipboard
{
    public class DataContent
    {

        Dictionary<string, object> data = new Dictionary<string, object>();

        public void SetData(DataPackageView package)
        {
            data.Clear();
            package.AvailableFormats.ToList().ForEach(f=>data.Add(f,package.GetDataAsync(f).GetAwaiter().GetResult()));
        }

        public T GetData<T>(string format) where  T:class
        {
            data.TryGetValue(format, out object value);
            return value as T;
        }
    }

    public class Clipboard
    {
        public Clipboard()
        {
            if (!Cp.IsHistoryEnabled())
            {
                Console.WriteLine("[+] Turning on clipboard history feature...");
                try
                {
                    var rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Clipboard", true);

                    if (rk == null)
                    {
                        Console.WriteLine(
                            "[!] Clipboard history feature not available on target! Target needs to be at least Win10 Build 1809.\n[!] Exiting...\n");
                        return;
                    }

                    rk.SetValue("EnableClipboardHistory", "1", RegistryValueKind.DWord);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            Cp.ContentChanged += async (s, e) =>
            {
                DataPackageView dataPackageView = Cp.GetContent();
                foreach (var availableFormat in dataPackageView.AvailableFormats)
                {
                    Console.WriteLine(availableFormat);
                }

                var a = await Cp.GetHistoryItemsAsync();
                var b = a.Items;
                foreach (var it in b)
                {
                    Console.WriteLine(it.Timestamp);
                }

                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    string text = await dataPackageView.GetTextAsync();
                    Console.WriteLine(text);
                }
            };
            Cp.HistoryChanged += async (s, e) => { Console.WriteLine("History Changed!"); };
        }

        public void CopyTo(Register register)
        {
            // void Handler(object s, object e)
            // {
            //     Cp.ContentChanged -= Handler;
            //     var dataPackageView = Cp.GetContent();
            //     var content = new DataContent() {View = dataPackageView};
            //     History.Insert(0, content);
            //     if (register == null) return;
            //
            //     register.Content = content;
            // }
            //
            // ;
            // Cp.ContentChanged += Handler;
            // Keyboard.Hit(Keys.C, new Keys[] {Keys.ControlKey});
        }

        public void PasteFrom(Register register)
        {
            // if (register == null) return;
            // var view = register.Content.View;
            // if (view == null) return;
            // var dataPackage = new DataPackage();
            // dataPackage.RequestedOperation = view.RequestedOperation;
            // view.Properties.ToList().ForEach(p => dataPackage.Properties.Add(p));
            //
            // view.AvailableFormats.ToList().ForEach(f =>
            // {
            //     
            //
            //     dataPackage.SetDataProvider(f, async request =>
            //     {
            //         DataProviderDeferral deferral = request.GetDeferral();
            //         try
            //         {
            //
            //             dataPackage.SetData(f, o);
            //         }
            //         catch (Exception e)
            //         {
            //             MessageBox.Show(e.Message);
            //         }
            //         finally
            //         {
            //             deferral.Complete();
            //
            //         }
            //     });
            //
            // });
            // Cp.SetContent(dataPackage);
            //
            // Keyboard.Hit(Keys.V, new Keys[] {Keys.ControlKey});
        }

        public Register A = new Register();
        public Register S = new Register();
        public Register D = new Register();
        public Register F = new Register();
        public Register G = new Register();

        public ObservableCollection<DataContent> History = new ObservableCollection<DataContent>();
    }
}
