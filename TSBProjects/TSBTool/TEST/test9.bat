:: test9.bat
:: Tests the TSBTool program
::
::
::@echo off

copy ..\bin\Debug\TSBToolSupreme.exe .
copy "..\bin\Debug\TSB 2007-32-111.nes" . 
.\TSBToolSupreme "TSB 2007-32-111.nes" Test9.txt -out:output.nes
.\TSBToolSupreme -j -n -f -a -s -sch -pb -of -proBowl output.nes > OUTPUT.txt
BCompare.exe .\Test9.txt .\OUTPUT.txt
::FC /C /L Test1.txt OUTPUT.txt > RESULTS.txt
::del *.nes

::start notepad  RESULTS.txt


