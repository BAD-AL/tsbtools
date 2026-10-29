@echo off
rem Runs the whole browser test suite in Edge (about 2 minutes). Extra arguments go to pytest, e.g.:
rem   run_tests.bat -k photos        only the photo tests
rem   run_tests.bat -x               stop at the first failure
cd /d "%~dp0.."
call tests\_python.cmd -m pytest tests -q -p no:cacheprovider %*
