:: Tests the TSBTool program
::
::
::@echo off

::copy ..\bin\RELEASE\*.exe .
copy ..\bin\Debug\TSBToolSupreme.exe .
copy ..\bin\Debug\TSPRBOWL.nes .
.\TSBToolSupreme.exe .\TSPRBOWL.nes Test1.txt -out:output.nes
.\TSBToolSupreme.exe output.nes > OUTPUT.txt
BCompare.exe .\Test1.txt .\OUTPUT.txt
::FC /C /L Test1.txt OUTPUT.txt > RESULTS.txt
::del *.nes

::start notepad  RESULTS.txt


