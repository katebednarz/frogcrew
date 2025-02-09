@echo off
setlocal

:: Require admin privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo This script requires administrative privileges. Run as administrator.
    pause
    exit /b
)

:: Store the path of the current script
set SCRIPT_DIR=%~dp0

:: Define a subdirectory "SQLInstall" in the same folder as this script
set INSTALL_DIR=%SCRIPT_DIR%SQLInstall

:: Create directory for installation files if it doesn't exist
if not exist "%INSTALL_DIR%" (
    mkdir "%INSTALL_DIR%"
)

:: Change into the installation directory
cd /d "%INSTALL_DIR%"

:: Download the SQL Server installer with -L to follow redirects
echo Downloading SQL Server installer...
curl -L -o SQLServer.exe https://go.microsoft.com/fwlink/?linkid=2216019

:: Create configuration file for silent install
echo Creating SQL Server configuration file...
(
    echo [OPTIONS]
    echo ACTION="Install"
    echo FEATURES=SQLENGINE
    echo INSTANCENAME="MSSQLSERVER"
    echo SECURITYMODE="SQL"
    echo SAPWD="YourStrongPassword"
    echo TCPENABLED=1
    echo IACCEPTSQLSERVERLICENSETERMS="True"
    echo QUIET="True"
    echo INSTANCEDIR="C:\Program Files\Microsoft SQL Server"
) > config.ini

echo Installing SQL Server silently...
.\SQLServer.exe /ConfigurationFile=config.ini /QUIET /IACCEPTSQLSERVERLICENSETERMS=TRUE

:: Verify installation
echo Checking if SQL Server service is running...
sc query MSSQLSERVER | findstr /i "RUNNING"
if %errorLevel% neq 0 (
    echo SQL Server installation failed or service is not running.
    pause
    exit /b
)

:: Download SQL command-line tools (SSMS) with -L to follow redirects
echo Downloading SQL Server command-line tools...
curl -L -o MsSQLTools.msi https://aka.ms/ssmsfullsetup
MsSQLTools.msi /quiet

:: Add SQL tools to system PATH
echo Adding SQL Tools to PATH...
setx PATH "%PATH%;C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn"

:: Restart SQL Server to apply changes
echo Restarting SQL Server...
net stop MSSQLSERVER /y
net start MSSQLSERVER

echo SQL Server installation completed successfully!
pause
exit /b
