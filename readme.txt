DFO Control Panel requires the .NET Framework version 3.5 SP1 or higher. If DFOCP.exe does not start, it probably means you don't have .NET 3.5 SP1 installed. You can download it here: http://www.microsoft.com/downloads/details.aspx?FamilyID=AB99342F-5D1A-413D-8319-81DA479AB0D7&displaylang=en

You may run DFO Control Panel from any directory, it should autodetect the directory that DFO is installed in. If it does not, you can put DFO Control Panel in your DFO directory or use the --dfodir= command-line option.

You are free to redistribute DFO Control Panel in binary form, provided that the LICENSE.txt, NDesk_LICENSE.txt, and NOTICE.txt files are kept in the distribution. The source code with Visual C# 2008 solution and project files is freely available under the Apache 2.0 license. The link for the latest version will be at http://code.google.com/p/dfocp/downloads/list. I encourage you to compile from source if you know how, to verify that the only place your password is going is Nexon.

BE SURE YOU TRUST WHERE YOU DOWNLOAD THIS FROM. I WILL NOT BE HELD RESPONSIBLE IF YOU GET SCAMMED BY SOFTWARE PRETENDING TO BE THE REAL DFO CONTROL PANEL. IF YOU DO NOT TRUST IT AND CANNOT GET THE SOURCE CODE FOR IT, DO NOT USE IT.


Simply double-click DFOCP.exe to use the graphical version. Your options (but not your password) will be saved so you don't have to reselect them every time. You may choose to have your username saved so you don't have to enter it ever time.

Your settings and a log file are saved in %appdata%\DFO Control Panel\. If you encounter strange behavior or think you have found a bug, make a copy of the log file once the program is no longer running, make sure your username and password are not in it (they *shouldn't* be, but it's possible I messed up), and include the log in your report. If the log file is large, you may wish to use http://pastie.org/. The log file gets overwritten each time you run the program, so make sure you make a copy of it right away. When making a bug report or support request, be sure to include what version of DFO Control Panel you are using. The version can be found by clicking Help->About.

Q: Why is the "switch soundpacks" box greyed out?

A: You must have a directory called SoundPacksCustom in your DFO directory with the foreign soundpacks you wish to use.


By clicking File->Save As Bat... you can export your username, password, and options to a .bat script. Once you save the .bat file, just double-click the .bat file to start DFO with the username, password, and options you had entered. You won't need to open DFOCP.exe anymore (but you still need to keep it on your computer). Make sure you understand the risks of storing a password in a file on your computer. You must leave the window that is created when double-clicking the .bat file open. It will close itself when DFO finishes.


If you wish to use DFO Control Panel from the command-line or from a script, you can see the command-line arguments accepted by running

DFOCP.exe -help

If command-line arguments are present, the command-line version will be assumed unless you specify the -gui switch. When using the GUI, command-line arguments have higher priority than saved settings.


DFO Control Panel Copyright 2010 Greg Najda