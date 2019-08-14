:: Tests the TSBTool program
::
::
::@echo off

copy ..\bin\Debug\TSBToolSupreme.exe .
copy ..\bin\Debug\TSB1.smc .
.\TSBToolSupreme TSB1.smc Test5.txt -out:output.smc
.\TSBToolSupreme -j -n -f -a -s -sch -pb -of output.smc > OUTPUT.txt
BCompare.exe .\Test5.txt .\OUTPUT.txt
::FC /C /L Test1.txt OUTPUT.txt > RESULTS.txt
::del *.nes

::start notepad  RESULTS.txt


