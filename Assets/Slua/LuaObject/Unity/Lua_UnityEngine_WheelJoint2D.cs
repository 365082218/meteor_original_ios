﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_WheelJoint2D : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetMotorTorque(IntPtr l) {
		try {
			UnityEngine.WheelJoint2D self=(UnityEngine.WheelJoint2D)checkSelf(l);
			System.Single a1;
			checkType(l,2,out a1);
			var ret=self.GetMotorTorque(a1);
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
	static public int get_suspension(IntPtr l) {
		try {
			UnityEngine.WheelJoint2D self=(UnityEngine.WheelJoint2D)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.suspension);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_suspension(IntPtr l) {
		try {
			UnityEngine.WheelJoint2D self=(UnityEngine.WheelJoint2D)checkSelf(l);
			UnityEngine.JointSuspension2D v;
			checkValueType(l,2,out v);
			self.suspension=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_useMotor(IntPtr l) {
		try {
			UnityEngine.WheelJoint2D self=(UnityEngine.WheelJoint2D)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.useMotor);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_useMotor(IntPtr l) {
		try {
			UnityEngine.WheelJoint2D self=(UnityEngine.WheelJoint2D)checkSelf(l);
			bool v;
			checkType(l,2,out v);
			self.useMotor=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_motor(IntPtr l) {
		try {
			UnityEngine.WheelJoint2D self=(UnityEngine.WheelJoint2D)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.motor);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_motor(IntPtr l) {
		try {
			UnityEngine.WheelJoint2D self=(UnityEngine.WheelJoint2D)checkSelf(l);
			UnityEngine.JointMotor2D v;
			checkValueType(l,2,out v);
			self.motor=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_jointTranslation(IntPtr l) {
		try {
			UnityEngine.WheelJoint2D self=(UnityEngine.WheelJoint2D)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.jointTranslation);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_jointSpeed(IntPtr l) {
		try {
			UnityEngine.WheelJoint2D self=(UnityEngine.WheelJoint2D)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.jointSpeed);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.WheelJoint2D");
		addMember(l,GetMotorTorque);
		addMember(l,"suspension",get_suspension,set_suspension,true);
		addMember(l,"useMotor",get_useMotor,set_useMotor,true);
		addMember(l,"motor",get_motor,set_motor,true);
		addMember(l,"jointTranslation",get_jointTranslation,null,true);
		addMember(l,"jointSpeed",get_jointSpeed,null,true);
		createTypeMetatable(l,null, typeof(UnityEngine.WheelJoint2D),typeof(UnityEngine.AnchoredJoint2D));
	}
}
