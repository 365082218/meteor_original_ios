﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Application : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			UnityEngine.Application o;
			o=new UnityEngine.Application();
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
	static public int Quit_s(IntPtr l) {
		try {
			UnityEngine.Application.Quit();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int CancelQuit_s(IntPtr l) {
		try {
			UnityEngine.Application.CancelQuit();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int Unload_s(IntPtr l) {
		try {
			UnityEngine.Application.Unload();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetStreamProgressForLevel_s(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,1,typeof(string))){
				System.String a1;
				checkType(l,1,out a1);
				var ret=UnityEngine.Application.GetStreamProgressForLevel(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(matchType(l,argc,1,typeof(int))){
				System.Int32 a1;
				checkType(l,1,out a1);
				var ret=UnityEngine.Application.GetStreamProgressForLevel(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function GetStreamProgressForLevel to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int CanStreamedLevelBeLoaded_s(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,1,typeof(string))){
				System.String a1;
				checkType(l,1,out a1);
				var ret=UnityEngine.Application.CanStreamedLevelBeLoaded(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(matchType(l,argc,1,typeof(int))){
				System.Int32 a1;
				checkType(l,1,out a1);
				var ret=UnityEngine.Application.CanStreamedLevelBeLoaded(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function CanStreamedLevelBeLoaded to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int CaptureScreenshot_s(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==1){
				System.String a1;
				checkType(l,1,out a1);
				UnityEngine.Application.CaptureScreenshot(a1);
				pushValue(l,true);
				return 1;
			}
			else if(argc==2){
				System.String a1;
				checkType(l,1,out a1);
				System.Int32 a2;
				checkType(l,2,out a2);
				UnityEngine.Application.CaptureScreenshot(a1,a2);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function CaptureScreenshot to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int HasProLicense_s(IntPtr l) {
		try {
			var ret=UnityEngine.Application.HasProLicense();
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
	static public int ExternalCall_s(IntPtr l) {
		try {
			System.String a1;
			checkType(l,1,out a1);
			System.Object[] a2;
			checkParams(l,2,out a2);
			UnityEngine.Application.ExternalCall(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int RequestAdvertisingIdentifierAsync_s(IntPtr l) {
		try {
			UnityEngine.Application.AdvertisingIdentifierCallback a1;
			checkDelegate(l,1,out a1);
			var ret=UnityEngine.Application.RequestAdvertisingIdentifierAsync(a1);
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
	static public int OpenURL_s(IntPtr l) {
		try {
			System.String a1;
			checkType(l,1,out a1);
			UnityEngine.Application.OpenURL(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetStackTraceLogType_s(IntPtr l) {
		try {
			UnityEngine.LogType a1;
			checkEnum(l,1,out a1);
			var ret=UnityEngine.Application.GetStackTraceLogType(a1);
			pushValue(l,true);
			pushEnum(l,(int)ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetStackTraceLogType_s(IntPtr l) {
		try {
			UnityEngine.LogType a1;
			checkEnum(l,1,out a1);
			UnityEngine.StackTraceLogType a2;
			checkEnum(l,2,out a2);
			UnityEngine.Application.SetStackTraceLogType(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int RequestUserAuthorization_s(IntPtr l) {
		try {
			UnityEngine.UserAuthorization a1;
			checkEnum(l,1,out a1);
			var ret=UnityEngine.Application.RequestUserAuthorization(a1);
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
	static public int HasUserAuthorization_s(IntPtr l) {
		try {
			UnityEngine.UserAuthorization a1;
			checkEnum(l,1,out a1);
			var ret=UnityEngine.Application.HasUserAuthorization(a1);
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
	static public int get_streamedBytes(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.streamedBytes);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_isPlaying(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.isPlaying);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_isEditor(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.isEditor);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_isWebPlayer(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.isWebPlayer);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_platform(IntPtr l) {
		try {
			pushValue(l,true);
			pushEnum(l,(int)UnityEngine.Application.platform);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_isMobilePlatform(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.isMobilePlatform);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_isConsolePlatform(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.isConsolePlatform);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_runInBackground(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.runInBackground);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_runInBackground(IntPtr l) {
		try {
			bool v;
			checkType(l,2,out v);
			UnityEngine.Application.runInBackground=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_dataPath(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.dataPath);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_streamingAssetsPath(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.streamingAssetsPath);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_persistentDataPath(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.persistentDataPath);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_temporaryCachePath(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.temporaryCachePath);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_srcValue(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.srcValue);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_absoluteURL(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.absoluteURL);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_unityVersion(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.unityVersion);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_version(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.version);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_installerName(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.installerName);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_bundleIdentifier(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.bundleIdentifier);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_installMode(IntPtr l) {
		try {
			pushValue(l,true);
			pushEnum(l,(int)UnityEngine.Application.installMode);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_sandboxType(IntPtr l) {
		try {
			pushValue(l,true);
			pushEnum(l,(int)UnityEngine.Application.sandboxType);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_productName(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.productName);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_companyName(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.companyName);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_cloudProjectId(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.cloudProjectId);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_targetFrameRate(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.targetFrameRate);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_targetFrameRate(IntPtr l) {
		try {
			int v;
			checkType(l,2,out v);
			UnityEngine.Application.targetFrameRate=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_systemLanguage(IntPtr l) {
		try {
			pushValue(l,true);
			pushEnum(l,(int)UnityEngine.Application.systemLanguage);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_backgroundLoadingPriority(IntPtr l) {
		try {
			pushValue(l,true);
			pushEnum(l,(int)UnityEngine.Application.backgroundLoadingPriority);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_backgroundLoadingPriority(IntPtr l) {
		try {
			UnityEngine.ThreadPriority v;
			checkEnum(l,2,out v);
			UnityEngine.Application.backgroundLoadingPriority=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_internetReachability(IntPtr l) {
		try {
			pushValue(l,true);
			pushEnum(l,(int)UnityEngine.Application.internetReachability);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_genuine(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.genuine);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_genuineCheckAvailable(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.Application.genuineCheckAvailable);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.Application");
		addMember(l,Quit_s);
		addMember(l,CancelQuit_s);
		addMember(l,Unload_s);
		addMember(l,GetStreamProgressForLevel_s);
		addMember(l,CanStreamedLevelBeLoaded_s);
		addMember(l,CaptureScreenshot_s);
		addMember(l,HasProLicense_s);
		addMember(l,ExternalCall_s);
		addMember(l,RequestAdvertisingIdentifierAsync_s);
		addMember(l,OpenURL_s);
		addMember(l,GetStackTraceLogType_s);
		addMember(l,SetStackTraceLogType_s);
		addMember(l,RequestUserAuthorization_s);
		addMember(l,HasUserAuthorization_s);
		addMember(l,"streamedBytes",get_streamedBytes,null,false);
		addMember(l,"isPlaying",get_isPlaying,null,false);
		addMember(l,"isEditor",get_isEditor,null,false);
		addMember(l,"isWebPlayer",get_isWebPlayer,null,false);
		addMember(l,"platform",get_platform,null,false);
		addMember(l,"isMobilePlatform",get_isMobilePlatform,null,false);
		addMember(l,"isConsolePlatform",get_isConsolePlatform,null,false);
		addMember(l,"runInBackground",get_runInBackground,set_runInBackground,false);
		addMember(l,"dataPath",get_dataPath,null,false);
		addMember(l,"streamingAssetsPath",get_streamingAssetsPath,null,false);
		addMember(l,"persistentDataPath",get_persistentDataPath,null,false);
		addMember(l,"temporaryCachePath",get_temporaryCachePath,null,false);
		addMember(l,"srcValue",get_srcValue,null,false);
		addMember(l,"absoluteURL",get_absoluteURL,null,false);
		addMember(l,"unityVersion",get_unityVersion,null,false);
		addMember(l,"version",get_version,null,false);
		addMember(l,"installerName",get_installerName,null,false);
		addMember(l,"bundleIdentifier",get_bundleIdentifier,null,false);
		addMember(l,"installMode",get_installMode,null,false);
		addMember(l,"sandboxType",get_sandboxType,null,false);
		addMember(l,"productName",get_productName,null,false);
		addMember(l,"companyName",get_companyName,null,false);
		addMember(l,"cloudProjectId",get_cloudProjectId,null,false);
		addMember(l,"targetFrameRate",get_targetFrameRate,set_targetFrameRate,false);
		addMember(l,"systemLanguage",get_systemLanguage,null,false);
		addMember(l,"backgroundLoadingPriority",get_backgroundLoadingPriority,set_backgroundLoadingPriority,false);
		addMember(l,"internetReachability",get_internetReachability,null,false);
		addMember(l,"genuine",get_genuine,null,false);
		addMember(l,"genuineCheckAvailable",get_genuineCheckAvailable,null,false);
		createTypeMetatable(l,constructor, typeof(UnityEngine.Application));
	}
}
