@echo off
rem Finds Python and runs it with the given arguments. Set PYTHON to override (e.g. set PYTHON=C:\Python314\python.exe).
if defined PYTHON ( "%PYTHON%" %* & exit /b %errorlevel% )
where py >nul 2>nul && ( py -3 %* & exit /b %errorlevel% )
where python >nul 2>nul && ( python %* & exit /b %errorlevel% )
if exist "%LOCALAPPDATA%\Programs\Python\Python314\python.exe" ( "%LOCALAPPDATA%\Programs\Python\Python314\python.exe" %* & exit /b %errorlevel% )
echo Python not found. Install Python 3 or set the PYTHON environment variable.
exit /b 9009
