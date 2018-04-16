using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[ProtoBuf.ProtoContract]
public abstract class VO 
{
	/**
	 * 成员变量名到变量标识码的映射表
	 * */
	protected Dictionary<string,byte> Mapping = new Dictionary<string, byte>(); 
	//成员标识码到成员名的映射表
	private Dictionary<byte,string> Id2NameMapping = new Dictionary<byte, string> ();

	/**
	 * 对成员变量 Mapping 进行赋值
	 * */
	protected abstract void initMapping();


	protected void initId2NameMapping(){
		foreach(string varName in Mapping.Keys){
			if(Mapping[varName]==null)
			{
				Debug.LogError(varName);
			}
			Id2NameMapping.Add (Mapping [varName], varName);
		}
	}
	/**
	 * 获取成员变量的标识码
	 * */
	public byte getVarId(string varName){
		if(Mapping.ContainsKey(varName)){
			return Mapping [varName];
		}
		return 0;
	}


	/**
	 * 获取成员变量名称
	 * */
	public string getVarName(byte varId){
		if(Id2NameMapping.Count == 0){
			initId2NameMapping ();
		}
		if(Id2NameMapping.ContainsKey(varId)){
			return Id2NameMapping [varId];
		}
		return null;
	}

	public void setVarValue(byte varId,object value){
		string varName = getVarName(varId);
		if(varName!=null){
			setVarValue (varName, value);
		}
	}

	public void setVarValue(string varName,object value){
		FieldInfo fieldInfo = this.GetType ().GetField (varName);
		object obj = fieldInfo.GetValue(this);
		if(obj!=null&&(obj is ArrayList)&&varName.IndexOf("m_")!=0)
		{
			setVarValue("m_"+varName,null);
		}
		try
		{
			fieldInfo.SetValue (this, value);
		}catch (Exception) {
			Debug.LogError(varName);
		}

	}

	public void UpdataValue(string varName,object value)
	{
		FieldInfo fieldInfo = this.GetType ().GetField (varName);
		object obj = fieldInfo.GetValue(this);
		if(obj!=null&&(obj is ArrayList)&&varName.IndexOf("m_")!=0)
		{
			UpdataSub("m_"+varName,null);
		}
		fieldInfo.SetValue (this, value);
	}

	public void UpdataSub(string varName,object value)
	{
		FieldInfo fieldInfo = this.GetType ().GetField (varName);
		object obj = fieldInfo.GetValue(this);
		fieldInfo.SetValue (this, value);
	}

	public object getVarValue(string varName){
		FieldInfo fieldInfo = this.GetType ().GetField (varName);
		return fieldInfo.GetValue (this);
	}
	public object getVarValue(byte varId){
		string varName = getVarName(varId);
		if (varName != null) {
			return getVarValue (varName);
		}
		return null;
	}

	public T getVarValue<T>(string varName){
		return (T)getVarValue (varName);
	}

	public T getVarValue<T>(byte varId){
		return (T)getVarValue (varId);
	}
	/**
	 * 
	 * 根据类名称反射创建实例对象
	 * */ 
	public static VO createVO(string clazzName){
		return (VO)Assembly.GetExecutingAssembly ().CreateInstance (clazzName);
	}
}


