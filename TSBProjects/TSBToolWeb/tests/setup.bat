@echo off
rem One-time setup: installs the test tools, then builds the WebAssembly runtime the tests need.
cd /d "%~dp0"
call _python.cmd -m pip install playwright pytest || exit /b 1
call publish.bat
