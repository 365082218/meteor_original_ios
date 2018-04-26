﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_ComputeBuffer : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			UnityEngine.ComputeBuffer o;
			if(argc==3){
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				o=new UnityEngine.ComputeBuffer(a1,a2);
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			else if(argc==4){
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				UnityEngine.ComputeBufferType a3;
				checkEnum(l,4,out a3);
				o=new UnityEngine.ComputeBuffer(a1,a2,a3);
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			return error(l,"New object failed.");
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int Dispose(IntPtr l) {
		try {
			UnityEngine.ComputeBuffer self=(UnityEngine.ComputeBuffer)checkSelf(l);
			self.Dispose();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int Release(IntPtr l) {
		try {
			UnityEngine.ComputeBuffer self=(UnityEngine.ComputeBuffer)checkSelf(l);
			self.Release();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetData(IntPtr l) {
		try {
			UnityEngine.ComputeBuffer self=(UnityEngine.ComputeBuffer)checkSelf(l);
			System.Array a1;
			checkType(l,2,out a1);
			self.SetData(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetCounterValue(IntPtr l) {
		try {
			UnityEngine.ComputeBuffer self=(UnityEngine.ComputeBuffer)checkSelf(l);
			System.UInt32 a1;
			checkType(l,2,out a1);
			self.SetCounterValue(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetData(IntPtr l) {
		try {
			UnityEngine.ComputeBuffer self=(UnityEngine.ComputeBuffer)checkSelf(l);
			System.Array a1;
			checkType(l,2,out a1);
			self.GetData(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetNativeBufferPtr(IntPtr l) {
		try {
			UnityEngine.ComputeBuffer self=(UnityEngine.ComputeBuffer)checkSelf(l);
			var ret=self.GetNativeBufferPtr();
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
	static public int CopyCount_s(IntPtr l) {
		try {
			UnityEngine.ComputeBuffer a1;
			checkType(l,1,out a1);
			UnityEngine.ComputeBuffer a2;
			checkType(l,2,out a2);
			System.Int32 a3;
			checkType(l,3,out a3);
			UnityEngine.ComputeBuffer.CopyCount(a1,a2,a3);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_count(IntPtr l) {
		try {
			UnityEngine.ComputeBuffer self=(UnityEngine.ComputeBuffer)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.count);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_stride(IntPtr l) {
		try {
			UnityEngine.ComputeBuffer self=(UnityEngine.ComputeBuffer)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.stride);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.ComputeBuffer");
		addMember(l,Dispose);
		addMember(l,Release);
		addMember(l,SetData);
		addMember(l,SetCounterValue);
		addMember(l,GetData);
		addMember(l,GetNativeBufferPtr);
		addMember(l,CopyCount_s);
		addMember(l,"count",get_count,null,true);
		addMember(l,"stride",get_stride,null,true);
		createTypeMetatable(l,constructor, typeof(UnityEngine.ComputeBuffer));
	}
}
