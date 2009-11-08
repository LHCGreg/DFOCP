@echo off
REM MAKE SURE YOU TRUST WHERE YOU GOT BrowserlessDFOcmd.exe FROM!!!
REM Make sure you understand the risks associated with storing your password in a file on your computer.

REM replace YourUsernameHere and YourPasswordHere in the line below with your actual username and password and remove the word REM in the two lines below below.
REM BrowserlessDFOcmd.exe YourUsernameHere YourPasswordHere
REM IF ERRORLEVEL 1 (goto :error) ELSE (goto :success)

REM You can remove the line below once you have put in your username and password.
echo You need to edit this file in notepad for it to be used!

:error
pause
:success