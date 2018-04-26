using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_AI_NavMesh : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			UnityEngine.AI.NavMesh o;
			o=new UnityEngine.AI.NavMesh();
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
	static public int Raycast_s(IntPtr l) {
		try {
			UnityEngine.Vector3 a1;
			checkType(l,1,out a1);
			UnityEngine.Vector3 a2;
			checkType(l,2,out a2);
			UnityEngine.AI.NavMeshHit a3;
			System.Int32 a4;
			checkType(l,4,out a4);
			var ret=UnityEngine.AI.NavMesh.Raycast(a1,a2,out a3,a4);
			pushValue(l,true);
			pushValue(l,ret);
			pushValue(l,a3);
			return 3;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int CalculatePath_s(IntPtr l) {
		try {
			UnityEngine.Vector3 a1;
			checkType(l,1,out a1);
			UnityEngine.Vector3 a2;
			checkType(l,2,out a2);
			System.Int32 a3;
			checkType(l,3,out a3);
			UnityEngine.AI.NavMeshPath a4;
			checkType(l,4,out a4);
			var ret=UnityEngine.AI.NavMesh.CalculatePath(a1,a2,a3,a4);
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
	static public int FindClosestEdge_s(IntPtr l) {
		try {
			UnityEngine.Vector3 a1;
			checkType(l,1,out a1);
			UnityEngine.AI.NavMeshHit a2;
			System.Int32 a3;
			checkType(l,3,out a3);
			var ret=UnityEngine.AI.NavMesh.FindClosestEdge(a1,out a2,a3);
			pushValue(l,true);
			pushValue(l,ret);
			pushValue(l,a2);
			return 3;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SamplePosition_s(IntPtr l) {
		try {
			UnityEngine.Vector3 a1;
			checkType(l,1,out a1);
			UnityEngine.AI.NavMeshHit a2;
			System.Single a3;
			checkType(l,3,out a3);
			System.Int32 a4;
			checkType(l,4,out a4);
			var ret=UnityEngine.AI.NavMesh.SamplePosition(a1,out a2,a3,a4);
			pushValue(l,true);
			pushValue(l,ret);
			pushValue(l,a2);
			return 3;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetAreaCost_s(IntPtr l) {
		try {
			System.Int32 a1;
			checkType(l,1,out a1);
			System.Single a2;
			checkType(l,2,out a2);
			UnityEngine.AI.NavMesh.SetAreaCost(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetAreaCost_s(IntPtr l) {
		try {
			System.Int32 a1;
			checkType(l,1,out a1);
			var ret=UnityEngine.AI.NavMesh.GetAreaCost(a1);
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
	static public int GetAreaFromName_s(IntPtr l) {
		try {
			System.String a1;
			checkType(l,1,out a1);
			var ret=UnityEngine.AI.NavMesh.GetAreaFromName(a1);
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
	static public int CalculateTriangulation_s(IntPtr l) {
		try {
			var ret=UnityEngine.AI.NavMesh.CalculateTriangulation();
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
	static public int get_AllAreas(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.AI.NavMesh.AllAreas);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_avoidancePredictionTime(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.AI.NavMesh.avoidancePredictionTime);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_avoidancePredictionTime(IntPtr l) {
		try {
			float v;
			checkType(l,2,out v);
			UnityEngine.AI.NavMesh.avoidancePredictionTime=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_pathfindingIterationsPerFrame(IntPtr l) {
		try {
			pushValue(l,true);
			pushValue(l,UnityEngine.AI.NavMesh.pathfindingIterationsPerFrame);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_pathfindingIterationsPerFrame(IntPtr l) {
		try {
			int v;
			checkType(l,2,out v);
			UnityEngine.AI.NavMesh.pathfindingIterationsPerFrame=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.AI.NavMesh");
		addMember(l,Raycast_s);
		addMember(l,CalculatePath_s);
		addMember(l,FindClosestEdge_s);
		addMember(l,SamplePosition_s);
		addMember(l,SetAreaCost_s);
		addMember(l,GetAreaCost_s);
		addMember(l,GetAreaFromName_s);
		addMember(l,CalculateTriangulation_s);
		addMember(l,"AllAreas",get_AllAreas,null,false);
		addMember(l,"avoidancePredictionTime",get_avoidancePredictionTime,set_avoidancePredictionTime,false);
		addMember(l,"pathfindingIterationsPerFrame",get_pathfindingIterationsPerFrame,set_pathfindingIterationsPerFrame,false);
		createTypeMetatable(l,constructor, typeof(UnityEngine.AI.NavMesh));
	}
}
