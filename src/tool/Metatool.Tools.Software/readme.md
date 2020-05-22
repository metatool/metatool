> TIP: to create a Desktop shortcut for a Store app in Windows 10    
> 1. Press the Win + R keys together to open the Run dialog and type shell:AppsFolder in the run box.
> 1. Hit the Enter key to open the Applications folder.
> 1. Now, drag and drop the desired app to the Desktop. Windows will create a new shortcut for it instantly!


## Example
* Folder: `AK +`  
  * file `Visual Studio &Code.lnk`: `AK + C` to start run the file shortcut.
  * file `Find with Everything.lnk`: `AK + F` to search with Everything.exe. 
   > Note: the first letter is 'F', '&' is not needed.  
  * file `Find with Everything(new window)#Shift+F$Handled=true, Description = open a new instance of Everyting.lnk` and search: `AK + Shift + F` to search with Everything.exe in a new instance window, trigger keys configured after '#', and properties after '$'. This is all properties you could config via name: `Abc#A+B,C$Handled=true, Enabled=true, Description=ab cs ef`  
  > Note: the default value of `Handled` is true, no need to config it explicitly.

## how to pass args
 you could config command args in the shortcut property(Alt+Enter) target field, and could also add complex arg in the Comment field: `{"Args":"${IShell.SelectedPathsOrCurrentDirectory}"}`

> use '_' before folder or file name to disable it. i.e. `_Notepad.lnk` would disable this link.

> `MetatoolDir` is a user scale environment variable, and could be used in shortcut's target property. i.e. `%metatooldir%\tools\Metatool.Tools.Software\software\everything\Everything.exe`