using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class DataDecoder
{

	private MemoryStream memoryStream;
	private VO entity;
	private int identity = 0;
	private int position = 0;

	private static Dictionary<int,string> VoIdentiferMap = new Dictionary<int, string> ();

	public static void setVOMapping(Dictionary<string,int> mapping){
		if(VoIdentiferMap.Count == 0){
			foreach (string varName in mapping.Keys) {
				VoIdentiferMap [mapping [varName]] = varName;
			}
		}
	}

	public int Identity{get{return identity;}}
	public int Position{ 
		get { return position;} 
	}
	public DataDecoder (byte[] data,VO entity = null){

	
		memoryStream = new MemoryStream (data);

		if(entity == null){

			int clazzIdentifer = readByte2 ();
			string clazzName = VoIdentiferMap [clazzIdentifer];
			//根据名称反射其对象
			entity = VO.createVO(clazzName);
		}
		this.entity = entity;
	}

	/**
	 * 将1个字节读成一个整数
	 * @return
	 */
	private int readByte1(){
		return (memoryStream.ReadByte() & 0xff);
	}
	/**
	 * 读取两个字节的整数
	 * 
	 * @param value
	 */
	private int readByte2() {
		int mask = 0xff;

		byte[] bytes = new byte[]{(byte)memoryStream.ReadByte(),(byte)memoryStream.ReadByte()};
		int value = bytes[1] & mask;
		value = (value<<8) | (bytes[0]&mask);
		return value;
	}

	public VO decode(){



		try{
			read();

			position = (int)memoryStream.Position;
			return entity;
		}finally{
			memoryStream.Close ();
		}
	}
	private void read(){
		bool loop = true;
		long endIndex = memoryStream.Length - 1;
		int dataType = 0;
		while(loop && memoryStream.Position<=endIndex){
			dataType = memoryStream.ReadByte ();
			if(dataType == (int)DataType.Object){
				readObject (entity);
			}else if(dataType == (int)DataType.Array){
				readArray (entity);
			}else if(dataType == (int)DataType.Int32 || dataType == (int)DataType.Int16
				|| dataType == (int)DataType.UInt8 || dataType == (int)DataType.UInt16 || dataType == (int)DataType.UInt32
			    || dataType == (int)DataType.Double  || dataType == (int)DataType.Long || dataType == (int)DataType.Float
				|| dataType == (int)DataType.Boolean){
				readPrimitive(entity,dataType);
			}else if(dataType == (int)DataType.StringShort){
				readStringShort(entity);
			}else if(dataType == (int)DataType.String){
				readString (entity);
			}else if(dataType == (int)DataType.StringLong){
				readStringLong(entity);
			}else if(dataType == (int)DataType.Split){
				loop = false;
			}
		}
	}

	private void readObject(object entity,VO item=null){
		int identity = 0;
		if (entity is VO) {//读标识
			identity = memoryStream.ReadByte ();
		}
		int position = (int)memoryStream.Position;
		byte[] data = new byte[memoryStream.Length - memoryStream.Position];

		memoryStream.Read (data, 0, data.Length);
		DataDecoder decoder = new DataDecoder (data,item);
		VO vo = decoder.decode();

		position += decoder.Position;
		memoryStream.Position = position;


		setVarValue (entity, identity, vo);
	}

	private void setVarValue(object entity , int identity,object varValue){

		if(entity is VO){
			((VO)entity).setVarValue ((byte)identity,varValue);
		}else{
			((ArrayList)entity).Add (varValue);
		}
	}
	private void readArray(VO entity){
		int identity = memoryStream.ReadByte ();
		int itemClazzIdentifer = readByte2();

		ArrayList array = new ArrayList ();
		bool loop = true;
		long endIndex = memoryStream.Length - 1;
		int dataType = 0;

		while(loop && memoryStream.Position<=endIndex){
			dataType = memoryStream.ReadByte ();
			if(dataType == (int)DataType.Object){
				string clazzName = VoIdentiferMap [itemClazzIdentifer];
				readObject (array, null);
			}else if(dataType == (int)DataType.Array){
				//				readArray ();
				continue; //暂不支持数组嵌套
			}else if(dataType == (int)DataType.Int32 || dataType == (int)DataType.Int16
				|| dataType == (int)DataType.UInt8 || dataType == (int)DataType.UInt16 || dataType == (int)DataType.UInt32
			    || dataType == (int)DataType.Double || dataType == (int)DataType.Long || dataType == (int)DataType.Float
				|| dataType == (int)DataType.Boolean){
				readPrimitive(array,dataType);
			}else if(dataType == (int)DataType.StringShort){
				readStringShort (array);
			}else if(dataType == (int)DataType.String){
				readString (array);
			}else if(dataType == (int)DataType.StringLong){
				readStringLong (array);
			}else if(dataType == (int)DataType.Split){
				loop = false;
			}
		}
		entity.setVarValue((byte)identity,array);
	}
	private void readPrimitive(object entity,int dataType){
		if(dataType == (int)DataType.Int32){
			readInt(entity);
		}else if(dataType == (int)DataType.Int16){
			readInt16(entity);
		}else if(dataType == (int)DataType.UInt8) {
			readUInt8(entity);
		}else if(dataType == (int)DataType.UInt16 ){
			readUInt16(entity);
		} else if(dataType == (int)DataType.UInt32){
			readUInt (entity);
		}else if(dataType == (int)DataType.Double){
			readDouble(entity);
		}else if(dataType == (int)DataType.Long){
			readLong(entity);
		}else if(dataType == (int)DataType.Float){
			readFloat(entity);
		}else if(dataType == (int)DataType.Boolean){
			readBoolean(entity);
		} 
	}

	private void readUInt(object entity){
		int identity = 0;
		if(entity is VO){
			identity = memoryStream.ReadByte ();
		}
		byte[] bytes = new byte[4];
		memoryStream.Read (bytes, 0, bytes.Length);

		uint value = BitConverter.ToUInt32 (bytes,0);
		setVarValue (entity, identity, value);
	}
	private void readInt(object entity){
		int identity = 0;
		if (entity is VO) {
			identity = memoryStream.ReadByte ();
		}
		byte[] bytes = new byte[4];
		memoryStream.Read (bytes, 0, bytes.Length);

		int value = BitConverter.ToInt32 (bytes,0);
		setVarValue (entity, identity, value);
	}
	private void readBoolean(object entity){
		int identity = 0;
		if (entity is VO) {
			identity = memoryStream.ReadByte ();
		}
		int value = memoryStream.ReadByte();
		setVarValue (entity, identity, value>0? true : false);
	}

	private void readUInt8(object entity){
		int identity = 0;
		if (entity is VO) {
			identity = memoryStream.ReadByte ();
		}
		int value = memoryStream.ReadByte();
		setVarValue (entity, identity, value);
	}
	private void readUInt16(object entity){
		int identity = 0;
		if (entity is VO) {
			identity = memoryStream.ReadByte ();
		}
		byte[] bytes = new byte[2];
		memoryStream.Read (bytes, 0, bytes.Length);

		int value = BitConverter.ToUInt16 (bytes,0);
		setVarValue (entity, identity, value);
	}
	private void readInt16(object entity){
		int identity = 0;
		if (entity is VO) {
			identity = memoryStream.ReadByte ();
		}
		byte[] bytes = new byte[2];
		memoryStream.Read (bytes, 0, bytes.Length);

		short value = BitConverter.ToInt16 (bytes,0);
		setVarValue (entity, identity, value);
	}

	private void readDouble(object entity){
		int identity = 0;
		if (entity is VO) {
			identity = memoryStream.ReadByte ();
		}
		byte[] bytes = new byte[8];
		memoryStream.Read (bytes, 0, bytes.Length);

		double value = BitConverter.ToDouble (bytes,0);
		setVarValue (entity, identity, value);
	}

	private void readLong(object entity){
		int identity = 0;
		if (entity is VO) {
			identity = memoryStream.ReadByte ();
		}
		byte[] bytes = new byte[8];
		memoryStream.Read (bytes, 0, bytes.Length);
		
		long value = BitConverter.ToInt64 (bytes,0);
		setVarValue (entity, identity, value);
	}

	private void readFloat(object entity){
		int identity = 0;
		if (entity is VO) {
			identity = memoryStream.ReadByte ();
		}
		byte[] bytes = new byte[4];
		memoryStream.Read (bytes, 0, bytes.Length);
		
		float value = BitConverter.ToSingle (bytes,0);
		setVarValue (entity, identity, value);
	}

	private void readStringShort(object entity){
		int identity = memoryStream.ReadByte ();
		int len = memoryStream.ReadByte();
		byte[] bytes = new byte[len];
		memoryStream.Read (bytes, 0, bytes.Length);
		string strValue = Encoding.UTF8.GetString (bytes);
		setVarValue (entity, identity, strValue);
	}

	private void readString(object entity){
		int identity = memoryStream.ReadByte ();
		byte[] bytes = new byte[2];
		memoryStream.Read (bytes, 0, bytes.Length);

		int len = BitConverter.ToUInt16 (bytes,0);
		bytes = new byte[len];
		memoryStream.Read (bytes, 0, bytes.Length);
		string strValue = Encoding.UTF8.GetString (bytes);
		setVarValue (entity, identity, strValue);
	}
	private void readStringLong(object entity){
		int identity = memoryStream.ReadByte ();
		byte[] bytes = new byte[4];
		memoryStream.Read (bytes, 0, bytes.Length);

		int len = BitConverter.ToInt32 (bytes,0);
		bytes = new byte[len];
		memoryStream.Read (bytes, 0, bytes.Length);
		string strValue = Encoding.UTF8.GetString (bytes);
		setVarValue (entity, identity, strValue);
	}

}


