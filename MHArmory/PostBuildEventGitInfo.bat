@echo off

rem Sets TARGET_PATH to script argument 1.
set TARGET_PATH=%1
rem Trims quotes in the TARGET_PATH variable.
set TARGET_PATH=%TARGET_PATH:"=%
set TARGET=%TARGET_PATH%\GitInfo.cs

rem Resets the file to its original committed content.
git checkout "%TARGET%"
