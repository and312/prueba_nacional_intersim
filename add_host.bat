@echo off
:: Solicitar privilegios de Administrador si no se tienen
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Este archivo debe ejecutarse como Administrador.
    echo Por favor, haz clic derecho sobre este archivo y selecciona "Ejecutar como Administrador".
    echo.
    pause
    exit /b
)

echo Agregando nacional.intersim al archivo hosts...
echo. >> %windir%\system32\drivers\etc\hosts
echo 2.25.133.206 nacional.intersim >> %windir%\system32\drivers\etc\hosts
echo [OK] Dominio nacional.intersim agregado con exito.
echo.
pause
