wpnest
======

Windows Phone client for controlling Nest thermostat. 
Track project progress at https://trello.com/board/wpnest/50bea98af6a901701a001d58

Getting Up and Running
------
You'll need the following:
* Visual Studio 2012 (Express Edition might be fine)
* [Visual Studio 2012 Update 2](http://aka.ms/vsupdate) - Needed for unit testing support 
* [Windows Phone SDK](https://dev.windowsphone.com/en-us/downloadsdk) - Just install all of them

Project Folders
------
**Builds** contains the XAP files submitted to the marketplace as well as the debug symbols.

**Libraries** contains third-party libraries used by wpnest

**packages** contains third-party libraries from NuGet used by wpnest

**WPNest** is the WP7 project. All code files reside here.

**WPNest.Test** is the unit test project. Code files are linked from the WPNest folder.

**WPNest.WP8** is the WP8 project. Code files are linked from the WPNest folder.

