﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Automation;
using System.Windows.Forms;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Metatool.Command;
using Metatool.Input;
using Metatool.Plugin;
using Metatool.Utils;
using static Metatool.Input.Key;
using AutomationElement = System.Windows.Automation.AutomationElement;

namespace Metatool.MetaKeyboard
{
    public class FileExplorer
    {
        static bool IsExplorerOrDialog(IKeyEventArgs e)
        {
            var c = Utils.Window.CurrentWindowClass;
            return "CabinetWClass" == c || "#32770" == c;
        }

        static bool IsExplorer(IKeyEventArgs e)
        {
            var c = Utils.Window.CurrentWindowClass;
            return "CabinetWClass" == c;
        }

        public IKey FocusFileItemsView = (LWin + F).Down(e =>
        {
            var winHandle  = Utils.Window.CurrentWindowHandle;
            var winElement = AutomationElement.FromHandle(winHandle);
            var listBoxEle = winElement.FindFirst(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ClassNameProperty, "UIItemsView"));
            var listBox = listBoxEle.GetCurrentPattern(SelectionPattern.Pattern) as SelectionPattern;
            var selectedItem = listBox?.Current.GetSelection().FirstOrDefault();
            if (selectedItem != null) selectedItem.SetFocus();
            else
            {
                var firstItem = listBoxEle.FindFirst(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "UIItem"));
                firstItem?.Select();
            }

            // using (var automation = new UIA3Automation())
            // {
            //     var h = Utils.Window.CurrentWindowHandle;
            //     var element = automation.FromHandle(h);
            //     var listBox = element.FindFirstDescendant(cf => cf.ByClassName("UIItemsView"))?.AsListBox();
            //     if (listBox?.SelectedItem != null) listBox.SelectedItem.Focus();
            //     else listBox?.Items.FirstOrDefault()?.Select();
            // }

            e.Handled = true;
        }, IsExplorerOrDialog, "Focus &File Items View");


        public IKey FocusNavigationTreeView = (LWin + N).Down(e =>
        {
            using (var automation = new UIA3Automation())
            {
                var h        = Utils.Window.CurrentWindowHandle;
                var element  = automation.FromHandle(h);
                var treeView = element.FindFirstDescendant(cf => cf.ByClassName("SysTreeView32"));
                treeView?.AsTree().SelectedTreeItem.Focus();
            }

            e.Handled = true;
        }, IsExplorerOrDialog, "Focus &Navigation Tree View");

        public IKey CopySelectedPath = (Caps + Pipe).Down(async e =>
        {
            var handle = Utils.Window.CurrentWindowHandle;
            var paths  = await Explorer.GetSelectedPath(handle);
            var r      = string.Join(';', paths);
            System.Windows.Clipboard.SetText(r);
            e.Handled = true;
        }, IsExplorerOrDialog, "Copy Selected Files Path");


        public IKey NewFile = (Ctrl + Alt + N).Hit(async e =>
        {
            const string newFileName = "NewFile";
            var          handle      = Utils.Window.CurrentWindowHandle;
            var          fullPath    = await Explorer.Path(handle);
            var          fileName    = newFileName;
            var          i           = 1;
            while (File.Exists(fullPath + "\\" + fileName))
            {
                fileName = newFileName + i++;
            }

            var file = File.Create(fullPath + "\\" + fileName);
            file.Close();
            var keyboard = Services.Get<IKeyboard>();
            Explorer.Select(handle, new[] {fileName});
            keyboard.Type(Keys.F2);
        }, IsExplorer, "&New File");

        public IKey ShowDesktopFolder = (LWin + D).Down(e =>
            {
                Explorer.Open(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                e.Handled = true;
            }
            , null, "Show &Desktop Folder");
    }
}