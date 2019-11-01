using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace Metatool.Plugin
{
    public static class UIAExt
    {
        private static ILogger log = Services.Get<ILogger>();
        public static void Select(this AutomationElement ele)
        {
            try
            {
                var itemPat = ele.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                itemPat?.Select();
            }
            catch (InvalidOperationException e)
            {
                log.LogWarning(e.Message);
            }
        }
    }
}
