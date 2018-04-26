using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_BoxCollider2D : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_size(IntPtr l) {
		try {
			UnityEngine.BoxCollider2D self=(UnityEngine.BoxCollider2D)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.size);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_size(IntPtr l) {
		try {
			UnityEngine.BoxCollider2D self=(UnityEngine.BoxCollider2D)checkSelf(l);
			UnityEngine.Vector2 v;
			checkType(l,2,out v);
			self.size=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.BoxCollider2D");
		addMember(l,"size",get_size,set_size,true);
		createTypeMetatable(l,null, typeof(UnityEngine.BoxCollider2D),typeof(UnityEngine.Collider2D));
	}
}
