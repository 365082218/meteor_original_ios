call basePath.bat
@echo off
set excelname=Weapon.xlsx
set excelpath=%designpath%\Xlsx
set exportJson=1

echo on
python %protobuferpath%\python_protoc\excel2proto.py %excelname% %excelpath% %protopath% %bytespath% %csharppath%  %luapath% %protobuferpath% %exportJson%
pause

