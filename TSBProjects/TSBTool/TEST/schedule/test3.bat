:: Tests the TSBTool program
::
::
::@echo off
copy "..\..\bin\Debug\TSBToolSupreme.exe*" .
copy "..\..\bin\Debug\TSPRBOWL.nes" .

.\TSBToolSupreme TSPRBOWL.nes Input3.txt -out:output.nes
.\TSBToolSupreme output.nes -sch > Output3.txt
::FC /C /L Test1.txt Output1.txt > RESULTS.txt
::del *.nes

BCompare.exe  Input3.txt Output3.txt


