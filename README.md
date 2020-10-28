# meteor_for_unity</br>
端游流星蝴蝶剑.net</br>用unity重新写的仿造工程</br>
使用unity2020打开</br>

第一次开启，会弹出是否生成Slua代码</br>
如果生成之后编译器报错</br>
需要删除Assets/Slua/LuaObject/Custom内的全部文件</br>
然后点击编辑器顶部菜单栏里的SLua子菜单里的All Clear清理Slua生成文件</br>
完毕后点Slua子菜单里的All Make手动解决一些报错</br>

游戏起始场景是Patch场景。</br>

在这个工程里，有读取原作所有格式美术资源的实现</br>
skc(模型+材质),bnc（骨架）,gmc/gmb(场景/道具模型),amb(动画关键帧数据)等的实现</br>
以及使用Unity的蒙皮组件，用自定义的动画系统控制骨骼过程</br>
