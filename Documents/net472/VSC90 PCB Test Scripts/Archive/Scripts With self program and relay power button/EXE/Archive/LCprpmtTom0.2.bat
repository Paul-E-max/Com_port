REM Set the device name
set DEVICE=18F67K22

REM Set the path to your hex file
set HEXFILE="C:\Users\paul-e\Downloads\VSC90FW\application.production.hex"

"C:\Program Files\Microchip\MPLABX\v6.05\mplab_platform\mplab_ipe\ipecmd" -TPPK4 -P18F67K22 -E -M -F%HEXFILE% -W3.3