using System;

/**
 * 
 * 数据类型定义
 * */
public enum DataType
{
	//默认占位符
	Default =0,
	//32位的整数
	Int32 = 1,
	//字符串
	String = 2,
	//对象
	Object = 3,
	//数组
	Array = 4,
	//8位的无符号整数
	UInt8 = 5,
		//16位的整数
	Int16 = 6,
		//16位的无符号整数
	UInt16 = 7,
		//32位的无符号整数
	UInt32 = 8,
		//双精度数
	Double = 9,
		//布尔型
	Boolean = 10,
		//短字符串
	StringShort = 11,
		//很长很长的字符串
	StringLong = 12,

	//长整型,64位整数
	Long = 14,
	//浮点数
	Float = 15,

	//数组和对象用的分隔符
	Split = 100,
}


