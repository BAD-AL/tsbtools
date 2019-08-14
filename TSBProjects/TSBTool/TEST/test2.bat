:: Tests the TSBTool program
::
::
::@echo off

copy ..\bin\Debug\TSBToolSupreme.exe .
copy ..\bin\Debug\TSPRBOWL.nes .
.\TSBToolSupreme TSPRBOWL.nes Test2.txt -out:output.nes
.\TSBToolSupreme output.nes > OUTPUT.txt
BCompare.exe .\Test2.txt .\OUTPUT.txt
::FC /C /L Test1.txt OUTPUT.txt > RESULTS.txt
::del *.nes

::start notepad  RESULTS.txt


