:: Tests the TSBTool program
::
::
::@echo off
copy "..\..\bin\Debug\TSBToolSupreme.exe*" .
copy "..\..\bin\Debug\TSPRBOWL.nes" .

.\TSBToolSupreme TSPRBOWL.nes Input4.txt -out:output.nes
.\TSBToolSupreme output.nes -sch > Output4.txt
::FC /C /L Test1.txt Output1.txt > RESULTS.txt
::del *.nes

BCompare.exe  Input4.txt Output4.txt


