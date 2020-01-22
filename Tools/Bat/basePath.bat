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
echo 表格路径:[%designpath%]
echo Unity工程路径:[%projectpath%]
echo protobuf工具路径:[%protobuferpath%]
echo proto协议导出路径:[%protopath%]
echo bytes数据文件导出路径:[%bytespath%]
echo C#文件导出路径:[%csharppath%]
echo lua文件导出路径:[%luapath%]

echo on