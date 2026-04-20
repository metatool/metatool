## current status
* @app/Metaseed.Metatool/ArgumentProcessor.cs:22  to show a the console window
* service\IService\Metatool.Service.CoreLib\SimpleConsoleLoggerProvider.cs to log into console
## Goal
> Originally I'm show the log in console, but now I want to show the log in a web ui log window.

* add a show/hide(toggle) log function(like the ShowSearch function @lib/Metaseed.WebUI/backend/WebViewHost.xaml.cs:104 ) that will show the log in a web ui log window.
* the show log function is triggered from a hotKey, enter+l in @app/Metaseed.Metatool/appsettings.json:45-46 , and handled in @app/Metaseed.Metatool/FunctionalKeys.cs:42-43
* UI: need to create one like the frontend of showing the hotkeys lib\Metaseed.WebUI\frontend\src\App.svelte, need to refactor the of original project structure: extract a component of Hotkeys).
        * the web Log UI has a config of maxinum of logs to show, i.e. 2000.
        * the UI need to be fast and high performance
  
## Architect
* the logic of LogSink and LoggerProvider is in the proj Metatool.Core and in a folder named Log
* the ShowLogs function is defined in proj Metatool.Service.UI in a new interface IMetaToolUI, implementation is in the Metatool.UI proj in a new class MetaToolUI