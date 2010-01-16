@echo off
REM MAKE SURE YOU TRUST WHERE YOU GOT DFOCP.exe FROM!!!
REM Make sure you understand the risks associated with storing your password in a file on your computer.

REM replace YourUsernameHere and YourPasswordHere in the line below with your actual username and password and remove the word REM in the two lines below below.
REM DFOCP.exe "--username=YourUsernameHere" "--password=YourPasswordHere" --closepopup --full --nosoundswitch
REM IF ERRORLEVEL 1 (goto :error) ELSE (goto :success)

REM You may replace --closepopup with --noclosepopup if you do not wish to close the popup at the end of the game.
REM You may replace --full with --windowed if you want to start in windowed mode.
REM You may replace --nosoundswitch with --soundswitch if you have a SoundPacksCustom directory in your DFO directory and want to use those foreign sounds.

REM You can remove the line below once you have put in your username and password.
echo You need to edit this file in notepad for it to be used!

:error
pause
:success