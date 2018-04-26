using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_VR_VRStats : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_gpuTimeLastFrame(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.VR.VRStats.gpuTimeLastFrame);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.VR.VRStats");
		addMember(l,"gpuTimeLastFrame",get_gpuTimeLastFrame,null,false);
		createTypeMetatable(l,null, typeof(UnityEngine.VR.VRStats));
	}
}
