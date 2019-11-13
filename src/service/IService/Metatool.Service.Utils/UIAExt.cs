using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation;
using Metatool.Utils;
using Microsoft.Extensions.Logging;

namespace Metatool.Service
{
    public static class UIA
    {
        private static ILogger log = Services.Get<ILogger<Object>>();
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
        public static AutomationElement[] SelectedItems(this AutomationElement ele)
        {
            try
            {
                var itemPat = ele.GetCurrentPattern(SelectionPattern.Pattern) as SelectionPattern;
                return itemPat?.Current.GetSelection();
            }
            catch (InvalidOperationException e)
            {
                log.LogWarning(e.Message);
            }
            return null;
        }

      
        public static AutomationElement FirstChild(this AutomationElement ele,
            Func<ConditionFactory, Condition> condition) => First(ele, TreeScope.Children, condition);

        public static AutomationElement FirstDecendent(this AutomationElement ele,
            Func<ConditionFactory, Condition> condition) => First(ele, TreeScope.Descendants, condition);

        public static AutomationElement First(this AutomationElement ele, TreeScope scope, Func<ConditionFactory, Condition> condition)

        {
            try
            {
                return ele.FindFirst(scope, condition(new ConditionFactory()));
            }
            catch (InvalidOperationException e)
            {
                log.LogWarning(e.Message);
            }
            return null;
        }

    }
}
