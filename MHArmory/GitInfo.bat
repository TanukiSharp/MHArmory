@echo off
set TARGET=%1\GitInfo.cs

echo %TARGET%

del "%TARGET%"
echo public static class GitInfo ^{ >> "%TARGET%"
echo public static string Value ^= ^@^">>"%TARGET%"
git rev-parse --abbrev-ref HEAD >> "%TARGET%"
git rev-parse HEAD >> "%TARGET%"
git config --get remote.origin.url >> "%TARGET%"
echo ^"; >> "%TARGET%"
echo ^} >> "%TARGET%"
