﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_FrictionJoint2D : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_maxForce(IntPtr l) {
		try {
			UnityEngine.FrictionJoint2D self=(UnityEngine.FrictionJoint2D)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.maxForce);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_maxForce(IntPtr l) {
		try {
			UnityEngine.FrictionJoint2D self=(UnityEngine.FrictionJoint2D)checkSelf(l);
			float v;
			checkType(l,2,out v);
			self.maxForce=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_maxTorque(IntPtr l) {
		try {
			UnityEngine.FrictionJoint2D self=(UnityEngine.FrictionJoint2D)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.maxTorque);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_maxTorque(IntPtr l) {
		try {
			UnityEngine.FrictionJoint2D self=(UnityEngine.FrictionJoint2D)checkSelf(l);
			float v;
			checkType(l,2,out v);
			self.maxTorque=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.FrictionJoint2D");
		addMember(l,"maxForce",get_maxForce,set_maxForce,true);
		addMember(l,"maxTorque",get_maxTorque,set_maxTorque,true);
		createTypeMetatable(l,null, typeof(UnityEngine.FrictionJoint2D),typeof(UnityEngine.AnchoredJoint2D));
	}
}
