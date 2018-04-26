using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_ParticleSystemVertexStreams : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.ParticleSystemVertexStreams");
		addMember(l,0,"None");
		addMember(l,1,"Position");
		addMember(l,2,"Normal");
		addMember(l,4,"Tangent");
		addMember(l,8,"Color");
		addMember(l,16,"UV");
		addMember(l,32,"UV2BlendAndFrame");
		addMember(l,64,"CenterAndVertexID");
		addMember(l,128,"Size");
		addMember(l,256,"Rotation");
		addMember(l,512,"Velocity");
		addMember(l,1024,"Lifetime");
		addMember(l,2048,"Custom1");
		addMember(l,4096,"Custom2");
		addMember(l,8192,"Random");
		addMember(l,2147483647,"All");
		LuaDLL.lua_pop(l, 1);
	}
}
