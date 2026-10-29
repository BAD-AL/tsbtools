@echo off
rem Only the byte-for-byte comparison against the desktop editor (TSBTool\bin\Debug\TSBToolSupreme.exe, or set TSBWEB_CLI).
cd /d "%~dp0.."
call tests\_python.cmd -m pytest tests\test_matches_desktop.py -q -p no:cacheprovider %*
