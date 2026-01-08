@echo off
echo ========================================
echo TESTE DO GERADOR DE LICENCAS
echo ========================================
echo.

cd LicenseGenerator\bin\Debug

if not exist LicenseGenerator.exe (
    echo ERRO: LicenseGenerator.exe nao encontrado!
    echo Por favor, compile o projeto LicenseGenerator primeiro.
    pause
    exit /b 1
)

echo Executando LicenseGenerator.exe...
echo.
echo Escolha opcao [1] para ver o Hardware ID desta maquina
echo.

LicenseGenerator.exe

pause
