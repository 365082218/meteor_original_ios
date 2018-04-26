﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Time : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			UnityEngine.Time o;
			o=new UnityEngine.Time();
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
	static public int get_time(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.time);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_timeSinceLevelLoad(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.timeSinceLevelLoad);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_deltaTime(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.deltaTime);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_fixedTime(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.fixedTime);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_unscaledTime(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.unscaledTime);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_unscaledDeltaTime(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.unscaledDeltaTime);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_fixedDeltaTime(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.fixedDeltaTime);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_fixedDeltaTime(IntPtr l) {
		try {
			float v;
			checkType(l,2,out v);
			UnityEngine.Time.fixedDeltaTime=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_maximumDeltaTime(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.maximumDeltaTime);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_maximumDeltaTime(IntPtr l) {
		try {
			float v;
			checkType(l,2,out v);
			UnityEngine.Time.maximumDeltaTime=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_smoothDeltaTime(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.smoothDeltaTime);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_maximumParticleDeltaTime(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.maximumParticleDeltaTime);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_maximumParticleDeltaTime(IntPtr l) {
		try {
			float v;
			checkType(l,2,out v);
			UnityEngine.Time.maximumParticleDeltaTime=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_timeScale(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.timeScale);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_timeScale(IntPtr l) {
		try {
			float v;
			checkType(l,2,out v);
			UnityEngine.Time.timeScale=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_frameCount(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.frameCount);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_renderedFrameCount(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.renderedFrameCount);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_realtimeSinceStartup(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.realtimeSinceStartup);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_captureFramerate(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Time.captureFramerate);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_captureFramerate(IntPtr l) {
		try {
			int v;
			checkType(l,2,out v);
			UnityEngine.Time.captureFramerate=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.Time");
		addMember(l,"time",get_time,null,false);
		addMember(l,"timeSinceLevelLoad",get_timeSinceLevelLoad,null,false);
		addMember(l,"deltaTime",get_deltaTime,null,false);
		addMember(l,"fixedTime",get_fixedTime,null,false);
		addMember(l,"unscaledTime",get_unscaledTime,null,false);
		addMember(l,"unscaledDeltaTime",get_unscaledDeltaTime,null,false);
		addMember(l,"fixedDeltaTime",get_fixedDeltaTime,set_fixedDeltaTime,false);
		addMember(l,"maximumDeltaTime",get_maximumDeltaTime,set_maximumDeltaTime,false);
		addMember(l,"smoothDeltaTime",get_smoothDeltaTime,null,false);
		addMember(l,"maximumParticleDeltaTime",get_maximumParticleDeltaTime,set_maximumParticleDeltaTime,false);
		addMember(l,"timeScale",get_timeScale,set_timeScale,false);
		addMember(l,"frameCount",get_frameCount,null,false);
		addMember(l,"renderedFrameCount",get_renderedFrameCount,null,false);
		addMember(l,"realtimeSinceStartup",get_realtimeSinceStartup,null,false);
		addMember(l,"captureFramerate",get_captureFramerate,set_captureFramerate,false);
		createTypeMetatable(l,constructor, typeof(UnityEngine.Time));
	}
}
