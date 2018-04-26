using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_RenderTargetIdentifier : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			UnityEngine.Rendering.RenderTargetIdentifier o;
			if(matchType(l,argc,2,typeof(UnityEngine.Rendering.BuiltinRenderTextureType))){
				UnityEngine.Rendering.BuiltinRenderTextureType a1;
				checkEnum(l,2,out a1);
				o=new UnityEngine.Rendering.RenderTargetIdentifier(a1);
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(string))){
				System.String a1;
				checkType(l,2,out a1);
				o=new UnityEngine.Rendering.RenderTargetIdentifier(a1);
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(int))){
				System.Int32 a1;
				checkType(l,2,out a1);
				o=new UnityEngine.Rendering.RenderTargetIdentifier(a1);
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Texture))){
				UnityEngine.Texture a1;
				checkType(l,2,out a1);
				o=new UnityEngine.Rendering.RenderTargetIdentifier(a1);
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			else if(argc==0){
				o=new UnityEngine.Rendering.RenderTargetIdentifier();
				pushValue(l,true);
				pushObject(l,o);
				return 2;
			}
			return error(l,"New object failed.");
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.Rendering.RenderTargetIdentifier");
		createTypeMetatable(l,constructor, typeof(UnityEngine.Rendering.RenderTargetIdentifier),typeof(System.ValueType));
	}
}
