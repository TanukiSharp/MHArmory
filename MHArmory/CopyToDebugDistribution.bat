@echo off
set TARGET=bin\DebugDistribution\

echo The content of directory %TARGET% will be deleted !!!

pause

rmdir /S /Q %TARGET%
mkdir %TARGET%
copy bin\Debug\*.dll %TARGET%
copy bin\Debug\*.pdb %TARGET%
copy bin\Debug\MHArmory.exe %TARGET%

pause
