@echo off
setlocal enabledelayedexpansion

REM Set the path to the MPLAB IPE command line tool
set IPECMD="C:\Program Files\Microchip\MPLABX\v6.05\mplab_platform\mplab_ipe\ipecmd.exe"

REM Set the path to your hex file
set HEXFILE="C:\WorkArea\1TS014_TEST_SEQUENCER (32)\EXE\application.production 1.hex"

REM Set the device name
set DEVICE=18F67K22

REM Debug messages
echo Debug: IPECMD Path = %IPECMD%
echo Debug: HEXFILE Path = %HEXFILE%
echo Debug: Device = %DEVICE%

REM Execute the programming command directly including the tool identifier
::set COMMAND=%IPECMD% -TPPK4 -P%DEVICE% -E -M -F%HEXFILE% -OL

:: comand for self powered
set COMMAND=%IPECMD% -TPPK4 -P%DEVICE% -E -M -F%HEXFILE% -W3.3 -OL


REM Debug message for the command
echo Debug: Command = %COMMAND%

%COMMAND%

REM Check the result
if %errorlevel% neq 0 (
    echo Programming failed
    exit /b %errorlevel%
) else (
    echo Programming succeeded
)
