using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_VR_InputTracking : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetLocalPosition_s(IntPtr l) {
		try {
			UnityEngine.VR.VRNode a1;
			checkEnum(l,1,out a1);
			var ret=UnityEngine.VR.InputTracking.GetLocalPosition(a1);
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
	static public int GetLocalRotation_s(IntPtr l) {
		try {
			UnityEngine.VR.VRNode a1;
			checkEnum(l,1,out a1);
			var ret=UnityEngine.VR.InputTracking.GetLocalRotation(a1);
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
	static public int Recenter_s(IntPtr l) {
		try {
			UnityEngine.VR.InputTracking.Recenter();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.VR.InputTracking");
		addMember(l,GetLocalPosition_s);
		addMember(l,GetLocalRotation_s);
		addMember(l,Recenter_s);
		createTypeMetatable(l,null, typeof(UnityEngine.VR.InputTracking));
	}
}
