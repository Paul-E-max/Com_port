::Date: 11/07/24
::Version: 1.06.01
:: FW for PIC, pointing on my local machine
@echo off
setlocal enabledelayedexpansion

REM Set the path to the MPLAB IPE command line tool
set IPECMD="C:\Program Files\Microchip\MPLABX\v6.05\mplab_platform\mplab_ipe\ipecmd.exe"

REM Set the path to your hex file
set HEXFILE="C:\Users\paul-e\Downloads\VSC90_IMAGE_BUILD1.06.01.hex"

REM Set the device name
set DEVICE=18F67K22

REM Debug messages
echo Debug: IPECMD Path = %IPECMD%
echo Debug: HEXFILE Path = %HEXFILE%
echo Debug: Device = %DEVICE%

REM Execute the programming command directly including the tool identifier
set PROGRAM_COMMAND=%IPECMD% -TPPK4 -P%DEVICE% -E -M -F%HEXFILE% -W3.3 -OL

REM Debug message for the programming command
echo Debug: Program Command = %PROGRAM_COMMAND%

%PROGRAM_COMMAND%

REM Check the result of the programming command
if %errorlevel% neq 0 (
    echo Programming failed
    exit /b %errorlevel%
) else (
    echo Programming succeeded
)

REM Command to turn off VDD after programming
set VDD_OFF_COMMAND=%IPECMD% -TPPK4 -P%DEVICE% -V0

REM Debug message for the VDD off command
echo Debug: VDD Off Command = %VDD_OFF_COMMAND%

%VDD_OFF_COMMAND%

REM Check the result of the VDD off command
if %errorlevel% neq 0 (
    echo Turning off VDD failed
    exit /b %errorlevel%
) else (
    echo VDD turned off successfully
)

REM Command to reset the PICkit (this might turn the LED back to blue)
set RESET_COMMAND=%IPECMD% -TPPK4 -P%DEVICE% -OL

REM Debug message for the reset command
echo Debug: Reset Command = %RESET_COMMAND%

%RESET_COMMAND%

REM Check the result of the reset command
if %errorlevel% neq 0 (
    echo Resetting the PICkit failed
    exit /b %errorlevel%
) else (
    echo PICkit reset successfully
)

endlocal
