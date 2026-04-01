# Feature: Focus the top most window of the selected screen
## Step 1
> the 2 functions are put in the 'Metatool.Utils' project as a class of ScreenManager, and expose via IScreen interface inside the Metatool.Service.Utils proj
1. Function of IdentifyScreens
* use win api to return all screens as a 2-dim array, every row may have different length. algorithm:
    * enum all screens from system, do it only one time in the ScreenManager's constructor
    * with location and height info of the screen, calculate the height center, if 2 screen's height center's difference is less than half height of the highest one, the 2 are considered in same row
    * the index start from the top,left screen, i.e. [0][0] is the left top screen

2. Function of Find the top most window on the screen and activate it, name: ActivateTopWindowOnScreen(int rowIndex, int columnIndex)
    * uses `MonitorFromWindow` API to determine which monitor a window belongs to (handles windows spanning monitors and minimized windows correctly)
    * filters to Alt+Tab-style windows only: skips invisible, cloaked, tool windows, noactivate, and owned windows
    * enumerates windows in Z-order (top to bottom), activates the first match
## Step 2
* add function into Metatool.Tools.KeyMouse proj with 3 hotkey config in the config.cs and config.json:
  * ActivateWindowOnScreen00 with `Tab+1`
  * ActivateWindowOnScreen01 with `Tab+2`
  * ActivateWindowOnScreen10 with `Tab+q`
* in the KeyboardMouse.cs, add hot key trigger handler to use the ActivateTopWindowOnScreen

## Implementation notes
* Class named `ScreenManager` (to avoid conflict with `System.Windows.Forms.Screen`), file: `ScreenManager.cs`
* Registered as singleton `IScreen -> ScreenManager` in `Metatool.Utils/Main.cs`
* Screen layout is cached once at construction time (no re-enumeration per hotkey press)
* Window-to-monitor matching uses `MonitorFromWindow` + `MonitorFromRect` (Win32 API) instead of manual rect intersection
* If no matching window is found on the target screen, the hotkey is silently ignored (logged as warning)

## Corner cases
* If a screen has no windows: activation is skipped (no-op, logged as warning)
* If screen indices are out of range: skipped with warning log
* Minimized windows: restored via `ShowWindowAsync(SW.Restore)` before activation