using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Profiling_Profiler : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			UnityEngine.Profiling.Profiler o;
			o=new UnityEngine.Profiling.Profiler();
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
	static public int AddFramesFromFile_s(IntPtr l) {
		try {
			System.String a1;
			checkType(l,1,out a1);
			UnityEngine.Profiling.Profiler.AddFramesFromFile(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int BeginSample_s(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==1){
				System.String a1;
				checkType(l,1,out a1);
				UnityEngine.Profiling.Profiler.BeginSample(a1);
				pushValue(l,true);
				return 1;
			}
			else if(argc==2){
				System.String a1;
				checkType(l,1,out a1);
				UnityEngine.Object a2;
				checkType(l,2,out a2);
				UnityEngine.Profiling.Profiler.BeginSample(a1,a2);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function BeginSample to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int EndSample_s(IntPtr l) {
		try {
			UnityEngine.Profiling.Profiler.EndSample();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetRuntimeMemorySize_s(IntPtr l) {
		try {
			UnityEngine.Object a1;
			checkType(l,1,out a1);
			var ret=UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(a1);
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
	static public int GetMonoHeapSize_s(IntPtr l) {
		try {
			var ret=UnityEngine.Profiling.Profiler.GetMonoHeapSize();
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
	static public int GetMonoUsedSize_s(IntPtr l) {
		try {
			var ret=UnityEngine.Profiling.Profiler.GetMonoUsedSize();
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
	static public int SetTempAllocatorRequestedSize_s(IntPtr l) {
		try {
			System.UInt32 a1;
			checkType(l,1,out a1);
			var ret=UnityEngine.Profiling.Profiler.SetTempAllocatorRequestedSize(a1);
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
	static public int GetTempAllocatorSize_s(IntPtr l) {
		try {
			var ret=UnityEngine.Profiling.Profiler.GetTempAllocatorSize();
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
	static public int GetTotalAllocatedMemory_s(IntPtr l) {
		try {
			var ret=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory();
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
	static public int GetTotalUnusedReservedMemory_s(IntPtr l) {
		try {
			var ret=UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemory();
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
	static public int GetTotalReservedMemory_s(IntPtr l) {
		try {
			var ret=UnityEngine.Profiling.Profiler.GetTotalReservedMemory();
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
	static public int get_supported(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Profiling.Profiler.supported);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_logFile(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Profiling.Profiler.logFile);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_logFile(IntPtr l) {
		try {
			string v;
			checkType(l,2,out v);
			UnityEngine.Profiling.Profiler.logFile=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_enableBinaryLog(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Profiling.Profiler.enableBinaryLog);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_enableBinaryLog(IntPtr l) {
		try {
			bool v;
			checkType(l,2,out v);
			UnityEngine.Profiling.Profiler.enableBinaryLog=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_enabled(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Profiling.Profiler.enabled);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_enabled(IntPtr l) {
		try {
			bool v;
			checkType(l,2,out v);
			UnityEngine.Profiling.Profiler.enabled=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_usedHeapSize(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Profiling.Profiler.usedHeapSize);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.Profiling.Profiler");
		addMember(l,AddFramesFromFile_s);
		addMember(l,BeginSample_s);
		addMember(l,EndSample_s);
		addMember(l,GetRuntimeMemorySize_s);
		addMember(l,GetMonoHeapSize_s);
		addMember(l,GetMonoUsedSize_s);
		addMember(l,SetTempAllocatorRequestedSize_s);
		addMember(l,GetTempAllocatorSize_s);
		addMember(l,GetTotalAllocatedMemory_s);
		addMember(l,GetTotalUnusedReservedMemory_s);
		addMember(l,GetTotalReservedMemory_s);
		addMember(l,"supported",get_supported,null,false);
		addMember(l,"logFile",get_logFile,set_logFile,false);
		addMember(l,"enableBinaryLog",get_enableBinaryLog,set_enableBinaryLog,false);
		addMember(l,"enabled",get_enabled,set_enabled,false);
		addMember(l,"usedHeapSize",get_usedHeapSize,null,false);
		createTypeMetatable(l,constructor, typeof(UnityEngine.Profiling.Profiler));
	}
}
