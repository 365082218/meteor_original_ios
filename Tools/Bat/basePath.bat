@echo off
title=excel2proto
mode con cols=100 lines=46
color 2

set designpath=..\
set projectpath=..\..\..\meteor_original_ios
set protobuferpath=..\..\Tools\ExcelToProto

set protopath=%projectpath%\Assets\Code\Datas\Proto
set bytespath=%projectpath%\Assets\Resources\Data
set csharppath=%projectpath%\Assets\Code\Datas
Set luapath=%projectpath%\Assets\LuaFramework\Lua\Datas
set exportJson=1

set path=%path%;G:\Python27
echo ���·��:[%designpath%]
echo Unity����·��:[%projectpath%]
echo protobuf����·��:[%protobuferpath%]
echo protoЭ�鵼��·��:[%protopath%]
echo bytes�����ļ�����·��:[%bytespath%]
echo C#�ļ�����·��:[%csharppath%]
echo lua�ļ�����·��:[%luapath%]

echo on