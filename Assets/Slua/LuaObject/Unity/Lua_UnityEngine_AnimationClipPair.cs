using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_AnimationClipPair : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			UnityEngine.AnimationClipPair o;
			o=new UnityEngine.AnimationClipPair();
			pushValue(l,true);
			pushValue(l,o);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_originalClip(IntPtr l) {
		try {
			UnityEngine.AnimationClipPair self=(UnityEngine.AnimationClipPair)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.originalClip);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_originalClip(IntPtr l) {
		try {
			UnityEngine.AnimationClipPair self=(UnityEngine.AnimationClipPair)checkSelf(l);
			UnityEngine.AnimationClip v;
			checkType(l,2,out v);
			self.originalClip=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_overrideClip(IntPtr l) {
		try {
			UnityEngine.AnimationClipPair self=(UnityEngine.AnimationClipPair)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.overrideClip);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_overrideClip(IntPtr l) {
		try {
			UnityEngine.AnimationClipPair self=(UnityEngine.AnimationClipPair)checkSelf(l);
			UnityEngine.AnimationClip v;
			checkType(l,2,out v);
			self.overrideClip=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.AnimationClipPair");
		addMember(l,"originalClip",get_originalClip,set_originalClip,true);
		addMember(l,"overrideClip",get_overrideClip,set_overrideClip,true);
		createTypeMetatable(l,constructor, typeof(UnityEngine.AnimationClipPair));
	}
}
