# meteor_for_unity</br>
端游流星蝴蝶剑.net</br>用unity重新写的仿造工程</br>
使用unity5.5.5p1打开</br>

第一次开启，会弹出是否生成Slua代码</br>
如果生成之后编译器报错</br>
需要删除Assets/Slua/LuaObject/Custom内的全部文件</br>
然后点击编辑器顶部菜单栏里的SLua子菜单里的All Clear清理Slua生成文件</br>
完毕后点Slua子菜单里的All Make即可</br>

游戏起始场景是Patch场景。这个场景是做更新的，但是现在没做。</br>
Patch会跳到Menu场景，这个是游戏主菜单场景。</br>
从在build设定存在的场景启动会自动跳转到Patch场景
