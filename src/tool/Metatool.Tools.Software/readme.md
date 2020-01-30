## To create a Desktop shortcut for a Store app in Windows 10

1. Press the Win + R keys together to open the Run dialog and type shell:AppsFolder in the run box.
1. Hit the Enter key to open the Applications folder.
1. Now, drag and drop the shortcut of the desired app to the Desktop. Windows will create a new shortcut for it instantly!


## Example
Folder: AK +
   Visual Studio &Code.lnk: AK + C to start run the file shortcut
   Find with Everything.lnk: AK + F to search with Everything. Note: the first letter is 'F'.
   Find with Everything(new window)#Shift+F$Handled=true, Description = open a new instance of Everyting and search: AK + Shift + F to search with Everything in new instance, trigger keys configed after #, and properties after $ 
   Notepade#$Handled=true: N to open notepad and mark this event handled

> Args: you could config command args in the shortcut property(Alt+Enter) target field, and could also add complex arg in the Comment field: `{"Args":"${IShell.SelectedPathsOrCurrentDirectory}"}`

> use '_' before folder or file name to disable it. i.e. _Notepade.lnk would disable this link.
