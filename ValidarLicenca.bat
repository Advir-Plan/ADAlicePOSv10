@echo off
echo ========================================
echo VALIDACAO DA LICENCA INSTALADA
echo ========================================
echo.

cd LicenseGenerator\bin\Debug

echo Validando: C:\Program Files\PRIMAVERA\SG100\ADAlicePOS.lic
echo.

echo 3 | LicenseGenerator.exe

pause
