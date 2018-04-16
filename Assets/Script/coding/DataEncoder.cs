using System;
using System.IO;
using System.Text; 
using System.Reflection;
using System.Collections;

using System.Collections.Generic;

public class DataEncoder
{

	//用来存放编码后的字节序列
	private MemoryStream memoryStream = new MemoryStream (100);

	private VO entity;

	private byte identity ;

	private static Dictionary<string,int> VoMapping;

	public static void setVOMapping(Dictionary<string,int> voMapping){
		if(VoMapping == null){
			VoMapping = voMapping;
		}
	}

	public DataEncoder (VO entity,byte identity=0,bool writeClazzIdentifer = true)
	{

		this.entity = entity;

		if(identity == 0){
			identity = (byte)DataType.Default;

		}else{//写入标识 ,当标识不等于DataType.Default时
			memoryStream.WriteByte (identity);
		}
		this.identity = identity;

		if (writeClazzIdentifer) {

			if(!VoMapping.ContainsKey(entity.GetType().Name))
			{
				int i=0;
			}
			int clazzIdentifer = VoMapping[entity.GetType().Name];
			writeByte2 (clazzIdentifer);
		}
	}
	/**
	 * 用2个字节写入一个整数
	 * */
	private void writeByte2(int value) {
		int mask = 0xff;
		memoryStream.WriteByte ((byte)((value) & mask));
		memoryStream.WriteByte ( (byte)((value>>8) & mask));
	}

	/**
	 * 
	 * 进行编码
	 * */
	public byte[] encode(){
		try{

			write (entity);

			memoryStream.Capacity = (int)memoryStream.Length;
			byte[] data = memoryStream.GetBuffer ();
			return data;
		}finally{
			memoryStream.Close ();
		}
	}

	private void write(VO entity){
		Type type = entity.GetType ();
		FieldInfo[] fields = type.GetFields ();
		foreach(FieldInfo fieldInfo in fields){
			if(!fieldInfo.IsPublic){
				continue;
			}
			bool primitive = fieldInfo.FieldType.IsPrimitive;
			string varName = fieldInfo.Name;
			string typeName = fieldInfo.FieldType.Name;

			if (fieldInfo.FieldType.IsPrimitive) {//int float double long 
				writePrimitive (entity,varName, typeName);
				continue;
			}


			//非基本类型: string  object  array 
			if(typeName == "String"){//字符串
				writeString (entity.getVarValue<string> (varName), entity.getVarId (varName));
				continue;
			}
			//集合类
			if (typeName == "ArrayList") {
				writeArray (entity.getVarValue<ArrayList> (varName),entity.getVarId (varName));
			}

			if(typeName == "VO"){
				writeObject (entity.getVarValue<VO> (varName),entity.getVarId (varName),true);
			}else{
				//通过BaseType来找
				typeName = fieldInfo.FieldType.BaseType.Name;
				if(typeName == "VO"){
					writeObject (entity.getVarValue<VO> (varName),entity.getVarId (varName),true);
				}
			}
			Console.WriteLine (varName+"  "+ typeName+"  "+(primitive ? "true" :"false"));
		}
	}
	/**
	 * 基本数据类型  int float double long 
	 * @param varName
	 * @param typeName
	 * */
	private void writePrimitive(VO entity ,string varName,string typeName){
		byte identity =  entity.getVarId (varName);
		if (typeName == "Int32") {

			int value = entity.getVarValue<int> (varName);
			if(value == 0)return;
			//			writeInt (entity.getVarValue<int> (varName), entity.getVarId (varName));
			if(value >=0){
				if(value<256){
					writeUInt8(value,identity);
				}else if(value<65536){
					writeUInt16(value,identity);
				}else {
					writeInt(value,identity);
				}
			}else{
				if(value>=-32768){
					writeInt16(value, identity);
				}else{
					writeInt(value, identity);
				}
			}
		}else if(typeName == "Double"){
			double value = entity.getVarValue<double> (varName);
			if(value == 0)return;
			writeDouble(value,identity);
		}else if(typeName == "Boolean"){
			bool value = entity.getVarValue<bool> (varName);
			writeBoolean(value,identity);
		}else if(typeName == "UInt16"){
			ushort value = entity.getVarValue<ushort> (varName);
			if(value == 0)return;
			writeUInt16(value,identity);
		}else if(typeName == "Int16"){
			short value = entity.getVarValue<short> (varName);
			if(value == 0)return;
			writeInt16(value,identity);
		}else if(typeName == "UInt32"){
			uint value = entity.getVarValue<uint> (varName);
			if(value == 0)return;
			writeUInt(value,identity);
		}else if(typeName == "Long"){
			long value = entity.getVarValue<long> (varName);
			if(value == 0)return;
			writeLong(value,identity);
		}else if(typeName == "Int64"){
			long value = entity.getVarValue<long> (varName);
			if(value == 0)return;
			writeLong(value,identity);
		}else if(typeName == "Float"){
			float value = entity.getVarValue<float>(varName);
			if(value == 0)return;
			writeFloat(value,identity);
		}

	}

	private void writePrimitive(object primitiveValue){
		var typeName = primitiveValue.GetType ().Name;
		if (typeName == "Int32") {
			//			writeInt ((int)primitiveValue,(byte)DataType.Default);

			int value = (int)primitiveValue;
			//			writeInt (entity.getVarValue<int> (varName), entity.getVarId (varName));
			if(value >=0){
				if(value<256){
					writeUInt8(value,(byte)DataType.Default);
				}else if(value<65536){
					writeUInt16(value,(byte)DataType.Default);
				}else {
					writeInt(value,(byte)DataType.Default);
				}
			}else{
				if(value>=-32768){
					writeInt16(value, (byte)DataType.Default);
				}else{
					writeInt(value, (byte)DataType.Default);
				}
			}
		}else if(typeName == "Double"){
			writeDouble((double)primitiveValue,(byte)DataType.Default);
		}else if(typeName == "Boolean"){
			writeBoolean((bool)primitiveValue,(byte)DataType.Default);
		}else if(typeName == "UInt16"){
			ushort value = (ushort)primitiveValue;
			writeUInt16(value,(byte)DataType.Default);
		}else if(typeName == "Int16"){
			short value = (short)primitiveValue;
			writeInt16(value,(byte)DataType.Default);
		}else if(typeName == "UInt32"){
			uint value = (uint)primitiveValue;
			writeUInt(value,(byte)DataType.Default);
		}else if(typeName == "Long"){
			long value = (long)primitiveValue;
			writeLong(value,(byte)DataType.Default);
		}else if(typeName == "Int64"){
			long value = (long)primitiveValue;
			writeLong(value,(byte)DataType.Default);
		}
		else if(typeName == "Float"){
			float value = (float)primitiveValue;
			writeFloat(value,(byte)DataType.Default);
		}
	}

	/**
	 * 整数编码
	 * @param intValue
	 * @param identity 标识码
	 * 规则:  数据类型+标识码+整数值
	 * 编码后占6个字节
	 */
	private void writeInt(int intValue,byte identity){

		memoryStream.WriteByte ((byte)DataType.Int32);
		memoryStream.WriteByte (identity);
		byte[] bytes = BitConverter.GetBytes (intValue);
		memoryStream.Write (bytes, 0, bytes.Length);

	}
	/**
	 * 整数编码
	 * @param value
	 * @param identity 标识码
	 * 规则:  数据类型+标识码+整数值
	 * 编码后占2个字节
	 */
	private void writeUInt8(int value,byte identity){
		memoryStream.WriteByte ((byte)DataType.UInt8);
		if(identity!=(byte)DataType.Default){
			memoryStream.WriteByte (identity);
		}

		memoryStream.WriteByte((byte)value);
	}
	private void writeUInt16(int value ,byte identity){
		memoryStream.WriteByte ((byte)DataType.UInt16);
		if (identity != (byte)DataType.Default) {
			memoryStream.WriteByte (identity);
		}
		byte[] bytes = BitConverter.GetBytes((ushort)value);
		memoryStream.Write (bytes,0,bytes.Length);
	}

	private void writeInt16(int value,byte identity){
		memoryStream.WriteByte ((byte)DataType.Int16);
		if (identity != (byte)DataType.Default) {
			memoryStream.WriteByte (identity);
		}
		byte[] bytes = BitConverter.GetBytes((short)value);
		memoryStream.Write (bytes,0,bytes.Length);
	}
	private void writeUInt(uint value, byte identity){
		memoryStream.WriteByte ((byte)DataType.UInt32);
		if (identity != (byte)DataType.Default) {
			memoryStream.WriteByte (identity);
		}
		byte[] bytes = BitConverter.GetBytes(value);
		memoryStream.Write (bytes,0,bytes.Length);
	}
	private void writeDouble(double value, byte identity){
		memoryStream.WriteByte ((byte)DataType.Double);
		if (identity != (byte)DataType.Default) {
			memoryStream.WriteByte (identity);
		}
		byte[] bytes = BitConverter.GetBytes(value);
		memoryStream.Write (bytes,0,bytes.Length);
	}

	private void writeLong(long value,byte identity){
		memoryStream.WriteByte ((byte)DataType.Long);
		if (identity != (byte)DataType.Default) {
			memoryStream.WriteByte (identity);
		}
		byte[] bytes = BitConverter.GetBytes((long)value);
		memoryStream.Write (bytes,0,bytes.Length);
	}
	private void writeFloat(float value,byte identity){
		memoryStream.WriteByte ((byte)DataType.Float);
		if (identity != (byte)DataType.Default) {
			memoryStream.WriteByte (identity);
		}
		byte[] bytes = BitConverter.GetBytes(value);
		memoryStream.Write (bytes,0,bytes.Length);
	}
	/**
	 * 字符串编码
	 * @param strValue 
	 * @param identity 标识码
	 * 规则： 数据类型+标识码+长度+字符串
	 * 编码后长度为字符串长度+6个字节
	 */
	private void writeString(string strValue , byte identity){
		if(strValue == null){
			return;
		}
		byte[] bytes = Encoding.UTF8.GetBytes (strValue);
		int strLen = bytes.Length;

		if(strLen<256){
			memoryStream.WriteByte((byte)DataType.StringShort);
			memoryStream.WriteByte(identity);

			memoryStream.WriteByte((byte)strLen);
		}else if(strLen<65536){

			memoryStream.WriteByte((byte)DataType.String);
			memoryStream.WriteByte(identity);

			byte[] lenBytes = BitConverter.GetBytes ((ushort)strLen);
			memoryStream.Write (lenBytes, 0, lenBytes.Length);
		}else {
			memoryStream.WriteByte((byte)DataType.StringLong);
			memoryStream.WriteByte(identity);

			byte[] lenBytes = BitConverter.GetBytes (strLen);
			memoryStream.Write (lenBytes, 0, lenBytes.Length);
		}
		memoryStream.Write (bytes, 0, bytes.Length);
	}
	private void writeBoolean(bool value,byte identity){
		memoryStream.WriteByte ((byte)DataType.Boolean);
		memoryStream.WriteByte (identity);
		memoryStream.WriteByte(value?(byte)1:(byte)0);
	}
	/**
	 * 数组编码
	 * @param arrValue 
	 * @param identity 标识码
	 * 规则： 数据类型+标识码+数组字节+分割符
	 * 编码后长度为数组字节+3个字节
	 */
	private void writeArray(ArrayList arrValue,byte identity){
		if(arrValue == null){
			return;
		}
		memoryStream.WriteByte ((byte)DataType.Array);
		memoryStream.WriteByte (identity);
		int itemIdentifer = -1;
		foreach(Object obj in arrValue){

			if (obj.GetType().IsPrimitive) {//int float double long bool
				if (itemIdentifer == -1) {
					string typeName = obj.GetType().Name;
					if(typeName == "Double"){
						itemIdentifer = (int)DataType.Double;
					}else if(typeName == "Boolean"){
						itemIdentifer = (int)DataType.Boolean;
					}else if(typeName == "UInt16"){
						itemIdentifer = (int)DataType.UInt16;
					}else if(typeName == "Int16"){
						itemIdentifer = (int)DataType.Int16;
					}else if(typeName == "UInt32"){
						itemIdentifer = (int)DataType.UInt32;
					}else if(typeName == "Long"){
						itemIdentifer = (int)DataType.Long;
					}else if(typeName == "Int64"){
						itemIdentifer = (int)DataType.Long;
					}else if(typeName == "Float"){
						itemIdentifer = (int)DataType.Float;
					}else if(typeName == "Int32"){
						itemIdentifer = (int)DataType.Int32;
					}else{
						//Does not support
					}
					writeByte2 (itemIdentifer);
				}
				writePrimitive (obj);
				continue;
			}
			if(obj is String){
				if (itemIdentifer == -1) {
					itemIdentifer = (int)DataType.String;
					writeByte2 (itemIdentifer);
				}
				writeString((string)obj,(byte)DataType.Default);
			}else if(obj is VO){
				if (itemIdentifer == -1) {


					string typeName = obj.GetType().Name;
					itemIdentifer = VoMapping[typeName];
					writeByte2 (itemIdentifer);
				}
				writeObject ((VO)obj,(byte)DataType.Default,true);
			}else{
				//Does not support
			}
		}

		memoryStream.WriteByte ((byte)DataType.Split);
	}

	/**
	 * 对象编码
	 * @param objValue 
	 * @param identity 标识码
	 * 规则： 数据类型+标识码+对象字节+分割符
	 * 编码后长度为对象字节+3个字节
	 */
	private void writeObject(VO objValue,byte identity = 0,bool writeClazzIdentifer=true){
		if(objValue == null){
			return;
		}
		//写入数据类型
		memoryStream.WriteByte ((byte)DataType.Object);
		//写入数据
		DataEncoder encoder = new DataEncoder (objValue, identity,writeClazzIdentifer);
		byte[] data = encoder.encode ();
		memoryStream.Write (data, 0, data.Length);
		//写入分隔符
		memoryStream.WriteByte ((byte)DataType.Split);
	}
}


