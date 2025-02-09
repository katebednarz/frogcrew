@echo off
setlocal

:: Define the .NET version
set DOTNET_VERSION=8.0
set DOTNET_INSTALLER=https://download.visualstudio.microsoft.com/download/pr/4b3b488c-9e69-4d60-bba2-79412b68d15d/b55f49a270c3413a6ea4b208f820515d/dotnet-sdk-8.0.405-win-x64.exe

:: Define download location
set INSTALLER_PATH=%TEMP%\dotnet-sdk-8.0.100-win-x64.exe

echo Downloading .NET %DOTNET_VERSION% SDK...
powershell -Command "& {Invoke-WebRequest -Uri '%DOTNET_INSTALLER%' -OutFile '%INSTALLER_PATH%'}"

if not exist "%INSTALLER_PATH%" (
    echo Failed to download .NET %DOTNET_VERSION%.
    exit /b 1
)

echo Installing .NET %DOTNET_VERSION% SDK...
start /wait "" "%INSTALLER_PATH%" /quiet /norestart

:: Verify Installation
dotnet --version
if %ERRORLEVEL% NEQ 0 (
    echo .NET installation failed.
    exit /b 1
)

echo .NET %DOTNET_VERSION% installed successfully.

:: Clean up
del /f /q "%INSTALLER_PATH%"

endlocal
exit /b 0
