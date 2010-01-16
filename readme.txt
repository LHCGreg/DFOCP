DFO Control Panel requires the .NET Framework version 3.5 or higher.

You may run DFO Control Panel from any directory, it should autodetect the directory that DFO is installed in. If it does not, you can put DFO Control Panel in your DFO directory or use the --dfodir= command-line option.

You are free to redistribute DFO Control Panel in binary form, provided that the LICENSE and NOTICE files are kept in the distribution. The source code with Visual C# 2008 solution and project files is freely available under the Apache 2.0 license. The link for the latest version will be at http://www.dfosource.com/forum/general-discussion/1831-release-browserless-dfo-launcher-post17604.html. I encourage you to compile from source if you know how, to verify that the only place your password is going is Nexon.

BE SURE YOU TRUST WHERE YOU DOWNLOAD THIS FROM. I WILL NOT BE HELD RESPONSIBLE IF YOU GET SCAMMED BY SOFTWARE PRETENDING TO BE THE REAL DFO CONTROL PANEL. IF YOU DO NOT TRUST IT AND CANNOT GET THE SOURCE CODE FOR IT, DO NOT USE IT.


Simply double-click DFOCP.exe to use the graphical version. Your options (but not your password) will be saved so you don't have to reselect them every time. You may choose to have your username saved so you don't have to enter it ever time.

Your settings and a log file are saved in %appdata%\DFO Control Panel\. If you encounter strange behavior or think you have found a bug, make a copy of the log file once the program is no longer running, make sure your username and password are not in it (they *shouldn't* be, but it's possible I messed up), and include the log in your report. If the log file is large, you may wish to use http://pastie.org/. The log file gets overwritten each time you run the program, so make sure you make a copy of it right away. When making a bug report or support request, be sure to include what version of DFO Control Panel you are using. The version can be found by clicking Help->About.

Q: Why is the "switch soundpacks" box greyed out?

A: You must have a directory called SoundPacksCustom in your DFO directory with the foreign soundpacks you wish to use.


BrowserlessDFO.bat is a batch file you can edit to put in your username and password so all you have to do is double-click BrowserlessDFO.bat without having to enter your username and password anymore. Make sure you understand the risks of storing a password in a file on your computer. If you choose to use this method, open BrowserlessDFO.bat in a text editor like Notepad, follow the directions inside, and save it. You can then double-click BrowerlessDFO.bat to log in without entering your username and password anymore. You must leave the window it creates open. It will close itself when DFO finishes.

Q: The instructions are hard. I don't get it!

A: Ask for help on the dfosource thread. Future versions will be able to generate BrowserlessDFO.bat for you.


If you wish to log in from the command-line or from a script, you can see the command-line arguments accepted by running

DFOCP.exe -help

If command-line arguments are present, the command-line version will be assumed unless you specify the -gui switch. When using the GUI, command-line arguments have higher priority than saved settings.


DFO Control Panel Copyright 2010 Greg Najda