﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_HingeJoint2D : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetMotorTorque(IntPtr l) {
		try {
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
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
	static public int get_useMotor(IntPtr l) {
		try {
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
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
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
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
	static public int get_useLimits(IntPtr l) {
		try {
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.useLimits);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_useLimits(IntPtr l) {
		try {
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
			bool v;
			checkType(l,2,out v);
			self.useLimits=v;
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
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
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
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
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
	static public int get_limits(IntPtr l) {
		try {
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.limits);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_limits(IntPtr l) {
		try {
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
			UnityEngine.JointAngleLimits2D v;
			checkValueType(l,2,out v);
			self.limits=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_limitState(IntPtr l) {
		try {
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
			pushValue(l,true);
			pushEnum(l,(int)self.limitState);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_referenceAngle(IntPtr l) {
		try {
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.referenceAngle);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_jointAngle(IntPtr l) {
		try {
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.jointAngle);
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
			UnityEngine.HingeJoint2D self=(UnityEngine.HingeJoint2D)checkSelf(l);
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
		getTypeTable(l,"UnityEngine.HingeJoint2D");
		addMember(l,GetMotorTorque);
		addMember(l,"useMotor",get_useMotor,set_useMotor,true);
		addMember(l,"useLimits",get_useLimits,set_useLimits,true);
		addMember(l,"motor",get_motor,set_motor,true);
		addMember(l,"limits",get_limits,set_limits,true);
		addMember(l,"limitState",get_limitState,null,true);
		addMember(l,"referenceAngle",get_referenceAngle,null,true);
		addMember(l,"jointAngle",get_jointAngle,null,true);
		addMember(l,"jointSpeed",get_jointSpeed,null,true);
		createTypeMetatable(l,null, typeof(UnityEngine.HingeJoint2D),typeof(UnityEngine.AnchoredJoint2D));
	}
}
