# Feature: Focus the top most window of the selected screen
## Step 1
> the 2 functions are put in the 'Metatool.Utils' project as a class of Screen, and expose these 2 function by IScreen class inside the Metatool.Service.Utils proj
1. Function of Identify Screen
* use win api to return all screens as a 2-dim array, every row may have different length. algorithm:
    * enum all screens from system
    * with location and height info of the screen, calculate the height center, if 2 screen's height center's difference is less than half height of the highest one, the 2 are considered in same row

2. Function of Find the top most window on the screen and activate it, name: ActivateTopWindowOnScreen(int rowIndex, int columnIndex)
## Step 2
* add function into Metatool.Tools.KeyMouse proj with 3 hotkey config in the config.cs and config.json:
  * ActivateWindowOnScreen00 with `Tab+1`
  * ActivateWindowOnScreen01 with `Tab+2`
  * ActivateWindowOnScreen10 with `Tab+q`
* in the KeyboardMouse.cs, add hot key trigger handler to use the ActivateTopWindowOnScreen
