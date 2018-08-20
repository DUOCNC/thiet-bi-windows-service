@ECHO OFF

REM The following directory is for .NET 4.0
set DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319
set PATH=%PATH%;%DOTNETFX2%

echo Uninstalling SMS_Service.exe...
echo ---------------------------------------------------
InstallUtil -u SMS_Service.exe
echo ---------------------------------------------------
echo Done.