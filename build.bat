@echo off
title Compilando KeyShield
echo Procurando o compilador do C# (csc.exe)...

set CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC_PATH%" (
    set CSC_PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
)

if not exist "%CSC_PATH%" (
    echo [ERRO] Nao foi possivel encontrar o csc.exe em nenhuma pasta do .NET Framework.
    echo Certifique-se de que o .NET Framework 4.0 ou superior esteja instalado.
    pause
    exit /b 1
)

echo Compilador encontrado: %CSC_PATH%
echo Compilando os arquivos C# com a logo embutida...

"%CSC_PATH%" /target:winexe /out:KeyShield.exe /resource:resources\logo.png,KeyShield.logo.png /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll src\*.cs

if %ERRORLEVEL% equ 0 (
    echo [SUCESSO] KeyShield.exe foi compilado com sucesso!
) else (
    echo [ERRO] Houve falhas durante a compilacao.
)

pause
