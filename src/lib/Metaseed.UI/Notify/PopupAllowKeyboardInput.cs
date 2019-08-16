using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;


namespace Metaseed.UI.Notify

{
    public class PopupAllowKeyboardInput
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(PopupAllowKeyboardInput),
                new PropertyMetadata(default(bool), IsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject d)
        {
            return (bool)d.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject d, bool value)
        {
            d.SetValue(IsEnabledProperty, value);
        }

        private static void IsEnabledChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs ea)
        {
            EnableKeyboardInput((Popup)sender, (bool)ea.NewValue);
        }

        private static void EnableKeyboardInput(Popup popup, bool enable)
        {
            if (enable)
            {
                IInputElement element = null;
                popup.Loaded += (sender, args) =>
                {
                    popup.Child.Focusable = true;
                    popup.Child.IsVisibleChanged += (o, ea) =>
                    {
                        if (popup.Child.IsVisible)
                        {
                            element = Keyboard.FocusedElement;
                            Keyboard.Focus(popup.Child);
                        }
                    };
                };
                // ReSharper disable ImplicitlyCapturedClosure  
                popup.Closed += (sender, args) => Keyboard.Focus(element);
            }
        }
    }
}