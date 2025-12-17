using System;
using Metatool.Input.MouseKeyHook.WinApi;

namespace Metatool.Input.MouseKeyHook.Implementation;

public partial class KeyboardState
{
    // Virtual Key Codes in VirtualKeyCode.cs
    // Keyboard state bit masks
    private const int VK_CAPITAL = 0x14; // Caps Lock
    private const int VK_NUMLOCK = 0x90; // Num Lock
    private const int VK_SCROLL = 0x91; // Scroll Lock
    private const byte KEY_PRESSED_BIT = 0x80; // Bit 7: Key is currently pressed
    private const byte KEY_TOGGLED_BIT = 0x01; // Bit 0: Toggle state (for toggle keys)
    private const short ASYNC_KEY_PRESSED = unchecked((short)0x8000); // High bit set

    /// <summary>
    /// NOTE: not work,
    /// as the KeyboardNativeMethods.GetAsyncKeyState(i); actually does not return real physical keyboard state for all keys.
    /// the same as GetKeyboardState.
    /// </summary>
    public static void ClearStuckKeys()
    {
        var keyState = new byte[256];
        // get state of current app
        KeyboardNativeMethods.GetKeyboardState(keyState);

        var needsUpdate = false;

        // Skip checking toggle keys (they're supposed to stay "on")
        int[] toggleKeys = [VK_CAPITAL, VK_NUMLOCK, VK_SCROLL];

        for (var i = 0; i < 256; i++)
        {
            // Skip toggle keys and undefined keys
            if (Array.IndexOf(toggleKeys, i) >= 0) continue;

            var statePressed = (keyState[i] & KEY_PRESSED_BIT) != 0;

            if (statePressed)
            {
                // check real keyboard state
                var asyncState = KeyboardNativeMethods.GetAsyncKeyState(i);
                var actuallyPressed = (asyncState & ASYNC_KEY_PRESSED) != 0;

                if (!actuallyPressed)
                {
                    // Clear pressed bit but keep toggle state
                    keyState[i] &= KEY_TOGGLED_BIT;
                    needsUpdate = true;

                    Console.WriteLine($"Cleared stuck key: {i} (0x{i:X2})");
                }
            }
        }

        if (needsUpdate)
        {
            KeyboardNativeMethods.SetKeyboardState(keyState);
        }
    }

    /// <summary>
    /// not work
    /// </summary>
    public static void ClearAllDownStateKeys()
    {
        var keyState = new byte[256];
        KeyboardNativeMethods.GetKeyboardState(keyState);

        var needsUpdate = false;

        // Skip checking toggle keys (they're supposed to stay "on")
        int[] toggleKeys = [VK_CAPITAL, VK_NUMLOCK, VK_SCROLL];

        for (var i = 0; i < 256; i++)
        {
            // Skip toggle keys and undefined keys
            if (Array.IndexOf(toggleKeys, i) >= 0) continue;

            var statePressed = (keyState[i] & KEY_PRESSED_BIT) != 0;

            if (statePressed)
            {
                // Clear pressed bit but keep toggle state
                keyState[i] &= KEY_TOGGLED_BIT;
                needsUpdate = true;

                Console.WriteLine($"Cleared stuck key: {i} (0x{i:X2})");

            }
        }
        if (needsUpdate)
        {
            KeyboardNativeMethods.SetKeyboardState(keyState);
        }
    }
}