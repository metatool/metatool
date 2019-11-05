using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation;

namespace Metatool.Plugin
{
    public class ConditionFactory
    {
        /// <summary>Creates a condition to search by an automation id.</summary>
        public PropertyCondition ByAutomationId(string automationId)
        {
            return new PropertyCondition(AutomationElement.AutomationIdProperty, (object) automationId);
        }

        /// <summary>
        /// Creates a condition to search by a <see cref="T:FlaUI.Core.Definitions.ControlType" />.
        /// </summary>
        public PropertyCondition ByControlType(ControlType controlType)
        {
            return new PropertyCondition(AutomationElement.ControlTypeProperty, (object) controlType);
        }

        /// <summary>Creates a condition to search by a class name.</summary>
        public PropertyCondition ByClassName(string className)
        {
            return new PropertyCondition(AutomationElement.ClassNameProperty, (object) className);
        }

        /// <summary>Creates a condition to search by a name.</summary>
        public PropertyCondition ByName(string name)
        {
            return new PropertyCondition(AutomationElement.NameProperty, (object) name);
        }

        /// <summary>
        /// Creates a condition to search by a text (same as <see cref="M:FlaUI.Core.Conditions.ConditionFactory.ByName(System.String)" />).
        /// </summary>
        public PropertyCondition ByText(string text)
        {
            return this.ByName(text);
        }

        /// <summary>Creates a condition to search by a process id.</summary>
        public PropertyCondition ByProcessId(int processId)
        {
            return new PropertyCondition(AutomationElement.ProcessIdProperty, (object) processId);
        }

        /// <summary>
        /// Creates a condition to search by a localized control type.
        /// </summary>
        public PropertyCondition ByLocalizedControlType(string localizedControlType)
        {
            return new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, (object) localizedControlType);
        }

        /// <summary>Creates a condition to search by a help text.</summary>
        public PropertyCondition ByHelpText(string helpText)
        {
            return new PropertyCondition(AutomationElement.HelpTextProperty, (object) helpText);
        }

        /// <summary>Searches for a Menu/MenuBar.</summary>
        public OrCondition Menu()
        {
            return new OrCondition((Condition) this.ByControlType(ControlType.Menu),
                (Condition) this.ByControlType(ControlType.MenuBar));
        }

        /// <summary>Searches for a DataGrid/List.</summary>
        public OrCondition Grid()
        {
            return new OrCondition((Condition) this.ByControlType(ControlType.DataGrid),
                (Condition) this.ByControlType(ControlType.List));
        }

        /// <summary>Searches for a horizontal scrollbar.</summary>
        public AndCondition HorizontalScrollBar()
        {
            return new AndCondition((Condition) this.ByControlType(ControlType.ScrollBar), (Condition) new OrCondition(
                new Condition[3]
                {
                    (Condition) this.ByAutomationId(nameof(HorizontalScrollBar)),
                    (Condition) this.ByAutomationId("Horizontal ScrollBar"),
                    (Condition) this.ByAutomationId("NonClientHorizontalScrollBar")
                }));
        }

        /// <summary>Searches for a vertical scrollbar.</summary>
        public AndCondition VerticalScrollBar()
        {
            return new AndCondition((Condition) this.ByControlType(ControlType.ScrollBar), (Condition) new OrCondition(
                new Condition[3]
                {
                    (Condition) this.ByAutomationId(nameof(VerticalScrollBar)),
                    (Condition) this.ByAutomationId("Vertical ScrollBar"),
                    (Condition) this.ByAutomationId("NonClientVerticalScrollBar")
                }));
        }
    }
}
