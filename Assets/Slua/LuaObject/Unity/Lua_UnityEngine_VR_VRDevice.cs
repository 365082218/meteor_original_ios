using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_VR_VRDevice : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetNativePtr_s(IntPtr l) {
		try {
			var ret=UnityEngine.VR.VRDevice.GetNativePtr();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_isPresent(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.VR.VRDevice.isPresent);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_model(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.VR.VRDevice.model);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_refreshRate(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.VR.VRDevice.refreshRate);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.VR.VRDevice");
		addMember(l,GetNativePtr_s);
		addMember(l,"isPresent",get_isPresent,null,false);
		addMember(l,"model",get_model,null,false);
		addMember(l,"refreshRate",get_refreshRate,null,false);
		createTypeMetatable(l,null, typeof(UnityEngine.VR.VRDevice));
	}
}
