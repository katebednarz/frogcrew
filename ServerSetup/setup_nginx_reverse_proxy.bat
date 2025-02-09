@echo off
setlocal

:: Define variables
set NGINX_DIR=C:\nginx
set NGINX_CONF=%NGINX_DIR%\conf\nginx.conf
set ASPNET_PORT=5228

:: Download and extract Nginx if not already installed
if not exist "%NGINX_DIR%" (
    echo Downloading Nginx...
    powershell -Command "& {Invoke-WebRequest -Uri 'https://nginx.org/download/nginx-1.25.3.zip' -OutFile 'nginx.zip'}"
    powershell -Command "& {Expand-Archive -Path 'nginx.zip' -DestinationPath 'C:\' -Force}"
    ren C:\nginx-1.25.3 nginx
    del nginx.zip
)

:: Stop Nginx if it's running
taskkill /F /IM nginx.exe >nul 2>&1

:: Create Nginx configuration
echo Updating Nginx Configuration...
(
echo worker_processes  1;
echo.
echo events {
echo     worker_connections  1024;
echo }
echo.
echo http {
echo     include       mime.types;
echo     default_type  application/octet-stream;
echo.
echo     sendfile        on;
echo     keepalive_timeout  65;
echo.
echo     server {
echo         listen 80;
echo         server_name localhost;
echo.
echo         location / {
echo             proxy_pass         http://localhost:%ASPNET_PORT%;
echo             proxy_http_version 1.1;
echo             proxy_set_header   Upgrade ^$http_upgrade;
echo             proxy_set_header   Connection keep-alive;
echo             proxy_set_header   Host ^$host;
echo             proxy_cache_bypass ^$http_upgrade;
echo             proxy_set_header   X-Forwarded-For ^$proxy_add_x_forwarded_for;
echo             proxy_set_header   X-Forwarded-Proto ^$scheme;
echo         }
echo     }
echo }
) > "%NGINX_CONF%"

:: Open firewall for Nginx (Port 80)
echo Configuring Windows Firewall...
netsh advfirewall firewall add rule name="Allow HTTP (Nginx)" dir=in action=allow protocol=TCP localport=80

:: Start Nginx
echo Starting Nginx...
cd %NGINX_DIR%
start nginx

echo Nginx setup is complete. Visit http://localhost/ to test.
pause
