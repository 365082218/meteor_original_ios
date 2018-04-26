﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_CommandBuffer : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			UnityEngine.Rendering.CommandBuffer o;
			o=new UnityEngine.Rendering.CommandBuffer();
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
	static public int Dispose(IntPtr l) {
		try {
			UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
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
			UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
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
	static public int Clear(IntPtr l) {
		try {
			UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
			self.Clear();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int DrawMesh(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==4){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Mesh a1;
				checkType(l,2,out a1);
				UnityEngine.Matrix4x4 a2;
				checkValueType(l,3,out a2);
				UnityEngine.Material a3;
				checkType(l,4,out a3);
				self.DrawMesh(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			else if(argc==5){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Mesh a1;
				checkType(l,2,out a1);
				UnityEngine.Matrix4x4 a2;
				checkValueType(l,3,out a2);
				UnityEngine.Material a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				self.DrawMesh(a1,a2,a3,a4);
				pushValue(l,true);
				return 1;
			}
			else if(argc==6){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Mesh a1;
				checkType(l,2,out a1);
				UnityEngine.Matrix4x4 a2;
				checkValueType(l,3,out a2);
				UnityEngine.Material a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				System.Int32 a5;
				checkType(l,6,out a5);
				self.DrawMesh(a1,a2,a3,a4,a5);
				pushValue(l,true);
				return 1;
			}
			else if(argc==7){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Mesh a1;
				checkType(l,2,out a1);
				UnityEngine.Matrix4x4 a2;
				checkValueType(l,3,out a2);
				UnityEngine.Material a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				System.Int32 a5;
				checkType(l,6,out a5);
				UnityEngine.MaterialPropertyBlock a6;
				checkType(l,7,out a6);
				self.DrawMesh(a1,a2,a3,a4,a5,a6);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function DrawMesh to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int DrawRenderer(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==3){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Renderer a1;
				checkType(l,2,out a1);
				UnityEngine.Material a2;
				checkType(l,3,out a2);
				self.DrawRenderer(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(argc==4){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Renderer a1;
				checkType(l,2,out a1);
				UnityEngine.Material a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				self.DrawRenderer(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			else if(argc==5){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Renderer a1;
				checkType(l,2,out a1);
				UnityEngine.Material a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				self.DrawRenderer(a1,a2,a3,a4);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function DrawRenderer to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int DrawProcedural(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==6){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Matrix4x4 a1;
				checkValueType(l,2,out a1);
				UnityEngine.Material a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				UnityEngine.MeshTopology a4;
				checkEnum(l,5,out a4);
				System.Int32 a5;
				checkType(l,6,out a5);
				self.DrawProcedural(a1,a2,a3,a4,a5);
				pushValue(l,true);
				return 1;
			}
			else if(argc==7){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Matrix4x4 a1;
				checkValueType(l,2,out a1);
				UnityEngine.Material a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				UnityEngine.MeshTopology a4;
				checkEnum(l,5,out a4);
				System.Int32 a5;
				checkType(l,6,out a5);
				System.Int32 a6;
				checkType(l,7,out a6);
				self.DrawProcedural(a1,a2,a3,a4,a5,a6);
				pushValue(l,true);
				return 1;
			}
			else if(argc==8){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Matrix4x4 a1;
				checkValueType(l,2,out a1);
				UnityEngine.Material a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				UnityEngine.MeshTopology a4;
				checkEnum(l,5,out a4);
				System.Int32 a5;
				checkType(l,6,out a5);
				System.Int32 a6;
				checkType(l,7,out a6);
				UnityEngine.MaterialPropertyBlock a7;
				checkType(l,8,out a7);
				self.DrawProcedural(a1,a2,a3,a4,a5,a6,a7);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function DrawProcedural to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int DrawProceduralIndirect(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==6){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Matrix4x4 a1;
				checkValueType(l,2,out a1);
				UnityEngine.Material a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				UnityEngine.MeshTopology a4;
				checkEnum(l,5,out a4);
				UnityEngine.ComputeBuffer a5;
				checkType(l,6,out a5);
				self.DrawProceduralIndirect(a1,a2,a3,a4,a5);
				pushValue(l,true);
				return 1;
			}
			else if(argc==7){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Matrix4x4 a1;
				checkValueType(l,2,out a1);
				UnityEngine.Material a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				UnityEngine.MeshTopology a4;
				checkEnum(l,5,out a4);
				UnityEngine.ComputeBuffer a5;
				checkType(l,6,out a5);
				System.Int32 a6;
				checkType(l,7,out a6);
				self.DrawProceduralIndirect(a1,a2,a3,a4,a5,a6);
				pushValue(l,true);
				return 1;
			}
			else if(argc==8){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Matrix4x4 a1;
				checkValueType(l,2,out a1);
				UnityEngine.Material a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				UnityEngine.MeshTopology a4;
				checkEnum(l,5,out a4);
				UnityEngine.ComputeBuffer a5;
				checkType(l,6,out a5);
				System.Int32 a6;
				checkType(l,7,out a6);
				UnityEngine.MaterialPropertyBlock a7;
				checkType(l,8,out a7);
				self.DrawProceduralIndirect(a1,a2,a3,a4,a5,a6,a7);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function DrawProceduralIndirect to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int DrawMeshInstanced(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==6){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Mesh a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				UnityEngine.Material a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				UnityEngine.Matrix4x4[] a5;
				checkArray(l,6,out a5);
				self.DrawMeshInstanced(a1,a2,a3,a4,a5);
				pushValue(l,true);
				return 1;
			}
			else if(argc==7){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Mesh a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				UnityEngine.Material a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				UnityEngine.Matrix4x4[] a5;
				checkArray(l,6,out a5);
				System.Int32 a6;
				checkType(l,7,out a6);
				self.DrawMeshInstanced(a1,a2,a3,a4,a5,a6);
				pushValue(l,true);
				return 1;
			}
			else if(argc==8){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Mesh a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				UnityEngine.Material a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				UnityEngine.Matrix4x4[] a5;
				checkArray(l,6,out a5);
				System.Int32 a6;
				checkType(l,7,out a6);
				UnityEngine.MaterialPropertyBlock a7;
				checkType(l,8,out a7);
				self.DrawMeshInstanced(a1,a2,a3,a4,a5,a6,a7);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function DrawMeshInstanced to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetRenderTarget(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==2){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Rendering.RenderTargetIdentifier a1;
				checkValueType(l,2,out a1);
				self.SetRenderTarget(a1);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(UnityEngine.Rendering.RenderTargetIdentifier))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Rendering.RenderTargetIdentifier a1;
				checkValueType(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				self.SetRenderTarget(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Rendering.RenderTargetIdentifier[]),typeof(UnityEngine.Rendering.RenderTargetIdentifier))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Rendering.RenderTargetIdentifier[] a1;
				checkArray(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				self.SetRenderTarget(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(int))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Rendering.RenderTargetIdentifier a1;
				checkValueType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				self.SetRenderTarget(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(int))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Rendering.RenderTargetIdentifier a1;
				checkValueType(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				self.SetRenderTarget(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(int),typeof(UnityEngine.CubemapFace))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Rendering.RenderTargetIdentifier a1;
				checkValueType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				UnityEngine.CubemapFace a3;
				checkEnum(l,4,out a3);
				self.SetRenderTarget(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(int),typeof(UnityEngine.CubemapFace),typeof(int))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Rendering.RenderTargetIdentifier a1;
				checkValueType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				UnityEngine.CubemapFace a3;
				checkEnum(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				self.SetRenderTarget(a1,a2,a3,a4);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(int),typeof(UnityEngine.CubemapFace))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Rendering.RenderTargetIdentifier a1;
				checkValueType(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				UnityEngine.CubemapFace a4;
				checkEnum(l,5,out a4);
				self.SetRenderTarget(a1,a2,a3,a4);
				pushValue(l,true);
				return 1;
			}
			else if(argc==6){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Rendering.RenderTargetIdentifier a1;
				checkValueType(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				UnityEngine.CubemapFace a4;
				checkEnum(l,5,out a4);
				System.Int32 a5;
				checkType(l,6,out a5);
				self.SetRenderTarget(a1,a2,a3,a4,a5);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function SetRenderTarget to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int Blit(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(UnityEngine.Rendering.RenderTargetIdentifier))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Rendering.RenderTargetIdentifier a1;
				checkValueType(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				self.Blit(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Texture),typeof(UnityEngine.Rendering.RenderTargetIdentifier))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Texture a1;
				checkType(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				self.Blit(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(UnityEngine.Material))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Rendering.RenderTargetIdentifier a1;
				checkValueType(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				UnityEngine.Material a3;
				checkType(l,4,out a3);
				self.Blit(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Texture),typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(UnityEngine.Material))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Texture a1;
				checkType(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				UnityEngine.Material a3;
				checkType(l,4,out a3);
				self.Blit(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(UnityEngine.Material),typeof(int))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Rendering.RenderTargetIdentifier a1;
				checkValueType(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				UnityEngine.Material a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				self.Blit(a1,a2,a3,a4);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Texture),typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(UnityEngine.Material),typeof(int))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				UnityEngine.Texture a1;
				checkType(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				UnityEngine.Material a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				self.Blit(a1,a2,a3,a4);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function Blit to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetTemporaryRT(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==4){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				self.GetTemporaryRT(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			else if(argc==5){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				self.GetTemporaryRT(a1,a2,a3,a4);
				pushValue(l,true);
				return 1;
			}
			else if(argc==6){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				UnityEngine.FilterMode a5;
				checkEnum(l,6,out a5);
				self.GetTemporaryRT(a1,a2,a3,a4,a5);
				pushValue(l,true);
				return 1;
			}
			else if(argc==7){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				UnityEngine.FilterMode a5;
				checkEnum(l,6,out a5);
				UnityEngine.RenderTextureFormat a6;
				checkEnum(l,7,out a6);
				self.GetTemporaryRT(a1,a2,a3,a4,a5,a6);
				pushValue(l,true);
				return 1;
			}
			else if(argc==8){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				UnityEngine.FilterMode a5;
				checkEnum(l,6,out a5);
				UnityEngine.RenderTextureFormat a6;
				checkEnum(l,7,out a6);
				UnityEngine.RenderTextureReadWrite a7;
				checkEnum(l,8,out a7);
				self.GetTemporaryRT(a1,a2,a3,a4,a5,a6,a7);
				pushValue(l,true);
				return 1;
			}
			else if(argc==9){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				System.Int32 a4;
				checkType(l,5,out a4);
				UnityEngine.FilterMode a5;
				checkEnum(l,6,out a5);
				UnityEngine.RenderTextureFormat a6;
				checkEnum(l,7,out a6);
				UnityEngine.RenderTextureReadWrite a7;
				checkEnum(l,8,out a7);
				System.Int32 a8;
				checkType(l,9,out a8);
				self.GetTemporaryRT(a1,a2,a3,a4,a5,a6,a7,a8);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function GetTemporaryRT to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int ReleaseTemporaryRT(IntPtr l) {
		try {
			UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
			System.Int32 a1;
			checkType(l,2,out a1);
			self.ReleaseTemporaryRT(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int ClearRenderTarget(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(argc==4){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Boolean a1;
				checkType(l,2,out a1);
				System.Boolean a2;
				checkType(l,3,out a2);
				UnityEngine.Color a3;
				checkType(l,4,out a3);
				self.ClearRenderTarget(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			else if(argc==5){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Boolean a1;
				checkType(l,2,out a1);
				System.Boolean a2;
				checkType(l,3,out a2);
				UnityEngine.Color a3;
				checkType(l,4,out a3);
				System.Single a4;
				checkType(l,5,out a4);
				self.ClearRenderTarget(a1,a2,a3,a4);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function ClearRenderTarget to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetGlobalFloat(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(int),typeof(float))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Single a2;
				checkType(l,3,out a2);
				self.SetGlobalFloat(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(string),typeof(float))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Single a2;
				checkType(l,3,out a2);
				self.SetGlobalFloat(a1,a2);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function SetGlobalFloat to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetGlobalVector(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(int),typeof(UnityEngine.Vector4))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				UnityEngine.Vector4 a2;
				checkType(l,3,out a2);
				self.SetGlobalVector(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(string),typeof(UnityEngine.Vector4))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				UnityEngine.Vector4 a2;
				checkType(l,3,out a2);
				self.SetGlobalVector(a1,a2);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function SetGlobalVector to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetGlobalColor(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(int),typeof(UnityEngine.Color))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				UnityEngine.Color a2;
				checkType(l,3,out a2);
				self.SetGlobalColor(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(string),typeof(UnityEngine.Color))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				UnityEngine.Color a2;
				checkType(l,3,out a2);
				self.SetGlobalColor(a1,a2);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function SetGlobalColor to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetGlobalMatrix(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(int),typeof(UnityEngine.Matrix4x4))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				UnityEngine.Matrix4x4 a2;
				checkValueType(l,3,out a2);
				self.SetGlobalMatrix(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(string),typeof(UnityEngine.Matrix4x4))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				UnityEngine.Matrix4x4 a2;
				checkValueType(l,3,out a2);
				self.SetGlobalMatrix(a1,a2);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function SetGlobalMatrix to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetGlobalFloatArray(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(string),typeof(System.Single[]))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Single[] a2;
				checkArray(l,3,out a2);
				self.SetGlobalFloatArray(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(int),typeof(System.Single[]))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Single[] a2;
				checkArray(l,3,out a2);
				self.SetGlobalFloatArray(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(string),typeof(List<System.Single>))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Collections.Generic.List<System.Single> a2;
				checkType(l,3,out a2);
				self.SetGlobalFloatArray(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(int),typeof(List<System.Single>))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Collections.Generic.List<System.Single> a2;
				checkType(l,3,out a2);
				self.SetGlobalFloatArray(a1,a2);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function SetGlobalFloatArray to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetGlobalVectorArray(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(string),typeof(UnityEngine.Vector4[]))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				UnityEngine.Vector4[] a2;
				checkArray(l,3,out a2);
				self.SetGlobalVectorArray(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(int),typeof(UnityEngine.Vector4[]))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				UnityEngine.Vector4[] a2;
				checkArray(l,3,out a2);
				self.SetGlobalVectorArray(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(string),typeof(List<UnityEngine.Vector4>))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Collections.Generic.List<UnityEngine.Vector4> a2;
				checkType(l,3,out a2);
				self.SetGlobalVectorArray(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(int),typeof(List<UnityEngine.Vector4>))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Collections.Generic.List<UnityEngine.Vector4> a2;
				checkType(l,3,out a2);
				self.SetGlobalVectorArray(a1,a2);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function SetGlobalVectorArray to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetGlobalMatrixArray(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(string),typeof(UnityEngine.Matrix4x4[]))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				UnityEngine.Matrix4x4[] a2;
				checkArray(l,3,out a2);
				self.SetGlobalMatrixArray(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(int),typeof(UnityEngine.Matrix4x4[]))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				UnityEngine.Matrix4x4[] a2;
				checkArray(l,3,out a2);
				self.SetGlobalMatrixArray(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(string),typeof(List<UnityEngine.Matrix4x4>))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Collections.Generic.List<UnityEngine.Matrix4x4> a2;
				checkType(l,3,out a2);
				self.SetGlobalMatrixArray(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(int),typeof(List<UnityEngine.Matrix4x4>))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Collections.Generic.List<UnityEngine.Matrix4x4> a2;
				checkType(l,3,out a2);
				self.SetGlobalMatrixArray(a1,a2);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function SetGlobalMatrixArray to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetGlobalTexture(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(int),typeof(UnityEngine.Rendering.RenderTargetIdentifier))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				self.SetGlobalTexture(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(string),typeof(UnityEngine.Rendering.RenderTargetIdentifier))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				UnityEngine.Rendering.RenderTargetIdentifier a2;
				checkValueType(l,3,out a2);
				self.SetGlobalTexture(a1,a2);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function SetGlobalTexture to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetGlobalBuffer(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(int),typeof(UnityEngine.ComputeBuffer))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				UnityEngine.ComputeBuffer a2;
				checkType(l,3,out a2);
				self.SetGlobalBuffer(a1,a2);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(string),typeof(UnityEngine.ComputeBuffer))){
				UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				UnityEngine.ComputeBuffer a2;
				checkType(l,3,out a2);
				self.SetGlobalBuffer(a1,a2);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function SetGlobalBuffer to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetShadowSamplingMode(IntPtr l) {
		try {
			UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
			UnityEngine.Rendering.RenderTargetIdentifier a1;
			checkValueType(l,2,out a1);
			UnityEngine.Rendering.ShadowSamplingMode a2;
			checkEnum(l,3,out a2);
			self.SetShadowSamplingMode(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int IssuePluginEvent(IntPtr l) {
		try {
			UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
			System.IntPtr a1;
			checkType(l,2,out a1);
			System.Int32 a2;
			checkType(l,3,out a2);
			self.IssuePluginEvent(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_name(IntPtr l) {
		try {
			UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.name);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_name(IntPtr l) {
		try {
			UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
			string v;
			checkType(l,2,out v);
			self.name=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_sizeInBytes(IntPtr l) {
		try {
			UnityEngine.Rendering.CommandBuffer self=(UnityEngine.Rendering.CommandBuffer)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.sizeInBytes);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.Rendering.CommandBuffer");
		addMember(l,Dispose);
		addMember(l,Release);
		addMember(l,Clear);
		addMember(l,DrawMesh);
		addMember(l,DrawRenderer);
		addMember(l,DrawProcedural);
		addMember(l,DrawProceduralIndirect);
		addMember(l,DrawMeshInstanced);
		addMember(l,SetRenderTarget);
		addMember(l,Blit);
		addMember(l,GetTemporaryRT);
		addMember(l,ReleaseTemporaryRT);
		addMember(l,ClearRenderTarget);
		addMember(l,SetGlobalFloat);
		addMember(l,SetGlobalVector);
		addMember(l,SetGlobalColor);
		addMember(l,SetGlobalMatrix);
		addMember(l,SetGlobalFloatArray);
		addMember(l,SetGlobalVectorArray);
		addMember(l,SetGlobalMatrixArray);
		addMember(l,SetGlobalTexture);
		addMember(l,SetGlobalBuffer);
		addMember(l,SetShadowSamplingMode);
		addMember(l,IssuePluginEvent);
		addMember(l,"name",get_name,set_name,true);
		addMember(l,"sizeInBytes",get_sizeInBytes,null,true);
		createTypeMetatable(l,constructor, typeof(UnityEngine.Rendering.CommandBuffer));
	}
}
