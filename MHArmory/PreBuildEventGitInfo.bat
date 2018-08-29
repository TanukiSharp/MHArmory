@echo off

rem Sets TARGET_PATH to script argument 1.
set TARGET_PATH=%1
rem Trims quotes in the TARGET_PATH variable.
set TARGET_PATH=%TARGET_PATH:"=%
set TARGET=%TARGET_PATH%\GitInfo.cs

rem Ensures target file is deleted before appending in it.
del "%TARGET%"

echo public static class GitInfo ^{ >> "%TARGET%"
echo public static string Branch ^= ^@^">>"%TARGET%"
git rev-parse --abbrev-ref HEAD >> "%TARGET%"
echo ^"; >> "%TARGET%"
echo public static string CommitHash ^= ^@^">>"%TARGET%"
git rev-parse HEAD >> "%TARGET%"
echo ^"; >> "%TARGET%"
echo public static string Repository ^= ^@^">>"%TARGET%"
git config --get remote.origin.url >> "%TARGET%"
echo ^"; >> "%TARGET%"
echo ^} >> "%TARGET%"
