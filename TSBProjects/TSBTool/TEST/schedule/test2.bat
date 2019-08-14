:: Tests the TSBTool program
::
::
::@echo off
copy "..\..\bin\Debug\TSBToolSupreme.exe*" .
copy "..\..\bin\Debug\TSPRBOWL.nes" .

.\TSBToolSupreme TSPRBOWL.nes Input2.txt -out:output.nes
.\TSBToolSupreme output.nes -sch > Output2.txt
::FC /C /L Test1.txt Output1.txt > RESULTS.txt
::del *.nes

BCompare.exe  Input2.txt Output2.txt


