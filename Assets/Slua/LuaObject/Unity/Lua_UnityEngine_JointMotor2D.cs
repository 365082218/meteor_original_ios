﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_JointMotor2D : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			UnityEngine.JointMotor2D o;
			o=new UnityEngine.JointMotor2D();
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
	static public int get_motorSpeed(IntPtr l) {
		try {
			UnityEngine.JointMotor2D self;
			checkValueType(l,1,out self);
			pushValue(l,true);
			pushValue(l,self.motorSpeed);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_motorSpeed(IntPtr l) {
		try {
			UnityEngine.JointMotor2D self;
			checkValueType(l,1,out self);
			float v;
			checkType(l,2,out v);
			self.motorSpeed=v;
			setBack(l,self);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_maxMotorTorque(IntPtr l) {
		try {
			UnityEngine.JointMotor2D self;
			checkValueType(l,1,out self);
			pushValue(l,true);
			pushValue(l,self.maxMotorTorque);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_maxMotorTorque(IntPtr l) {
		try {
			UnityEngine.JointMotor2D self;
			checkValueType(l,1,out self);
			float v;
			checkType(l,2,out v);
			self.maxMotorTorque=v;
			setBack(l,self);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.JointMotor2D");
		addMember(l,"motorSpeed",get_motorSpeed,set_motorSpeed,true);
		addMember(l,"maxMotorTorque",get_maxMotorTorque,set_maxMotorTorque,true);
		createTypeMetatable(l,constructor, typeof(UnityEngine.JointMotor2D),typeof(System.ValueType));
	}
}
