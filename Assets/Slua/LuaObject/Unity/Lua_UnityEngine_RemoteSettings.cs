﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_RemoteSettings : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			UnityEngine.RemoteSettings o;
			o=new UnityEngine.RemoteSettings();
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
	static public int CallOnUpdate_s(IntPtr l) {
		try {
			UnityEngine.RemoteSettings.CallOnUpdate();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetInt_s(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==1){
				System.String a1;
				checkType(l,1,out a1);
				var ret=UnityEngine.RemoteSettings.GetInt(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==2){
				System.String a1;
				checkType(l,1,out a1);
				System.Int32 a2;
				checkType(l,2,out a2);
				var ret=UnityEngine.RemoteSettings.GetInt(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function GetInt to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetFloat_s(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==1){
				System.String a1;
				checkType(l,1,out a1);
				var ret=UnityEngine.RemoteSettings.GetFloat(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==2){
				System.String a1;
				checkType(l,1,out a1);
				System.Single a2;
				checkType(l,2,out a2);
				var ret=UnityEngine.RemoteSettings.GetFloat(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function GetFloat to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetString_s(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==1){
				System.String a1;
				checkType(l,1,out a1);
				var ret=UnityEngine.RemoteSettings.GetString(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==2){
				System.String a1;
				checkType(l,1,out a1);
				System.String a2;
				checkType(l,2,out a2);
				var ret=UnityEngine.RemoteSettings.GetString(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function GetString to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetBool_s(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==1){
				System.String a1;
				checkType(l,1,out a1);
				var ret=UnityEngine.RemoteSettings.GetBool(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==2){
				System.String a1;
				checkType(l,1,out a1);
				System.Boolean a2;
				checkType(l,2,out a2);
				var ret=UnityEngine.RemoteSettings.GetBool(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function GetBool to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int HasKey_s(IntPtr l) {
		try {
			System.String a1;
			checkType(l,1,out a1);
			var ret=UnityEngine.RemoteSettings.HasKey(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.RemoteSettings");
		addMember(l,CallOnUpdate_s);
		addMember(l,GetInt_s);
		addMember(l,GetFloat_s);
		addMember(l,GetString_s);
		addMember(l,GetBool_s);
		addMember(l,HasKey_s);
		createTypeMetatable(l,constructor, typeof(UnityEngine.RemoteSettings));
	}
}
