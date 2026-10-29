@echo off
rem Fast check (about 40 seconds): the JS logic tests and the TSB3 photo tests.
cd /d "%~dp0.."
call tests\_python.cmd -m pytest tests\test_units.py tests\test_photos.py -q -p no:cacheprovider %*
