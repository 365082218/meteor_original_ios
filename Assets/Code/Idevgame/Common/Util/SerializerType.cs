using UnityEngine;
[System.Serializable]
[ProtoBuf.ProtoContract]
public struct MyVector
{
	public MyVector(float a, float b, float c)
	{
		x = a;
		y = b;
		z = c;
	}
	[ProtoBuf.ProtoMember(1)]
	public float x;
	[ProtoBuf.ProtoMember(2)]
	public float y;
	[ProtoBuf.ProtoMember(3)]
	public float z;
	public MyVector Scale(float scale) {
        MyVector clone = new MyVector();
        clone.x = x * scale;
        clone.y = y * scale;
        clone.z = z * scale;
        return clone;
    }
	public static implicit operator Vector3(MyVector a)
	{
		return new Vector3(a.x, a.y, a.z);
	}
	
	public static implicit operator MyVector(Vector3 a)
	{
		return new MyVector(a.x, a.y, a.z);
	}

    public static Vector3 operator -(MyVector a, MyVector b)
    {
        return new Vector3(a.x, a.y, a.z) - new Vector3(b.x, b.y, b.z);
    }

    public static MyVector operator +(MyVector a, MyVector b) {
        return new Vector3(a.x, a.y, a.z) + new Vector3(b.x, b.y, b.z);
    }
}
[System.Serializable]
[ProtoBuf.ProtoContract]
public struct MyVector2
{
    public MyVector2(float a, float b)
    {
        x = a;
        y = b;
    }
    [ProtoBuf.ProtoMember(1)]
    public float x;
    [ProtoBuf.ProtoMember(2)]
    public float y;

    public static implicit operator Vector2(MyVector2 a)
    {
        return new Vector3(a.x, a.y);
    }

    public static implicit operator MyVector2(Vector2 a)
    {
        return new MyVector2(a.x, a.y);
    }

    public static bool operator ==(MyVector2 lhs, MyVector2 rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y;
    }
    public static bool operator !=(MyVector2 lhs, MyVector2 rhs)
    {
        return lhs.x != rhs.x || lhs.y != rhs.y;
    }
    public static bool Equals(MyVector2 lhs, MyVector2 rhs) {
        return lhs.x == rhs.x && lhs.y == rhs.y;
    }
    public static MyVector2 Zero = Vector2.zero;
}
[System.Serializable]
[ProtoBuf.ProtoContract]
public struct MyQuaternion
{
    public MyQuaternion(float w1, float x1, float y1, float z1)
    {
        w = w1;
        x = x1;
        y = y1;
        z = z1;
    }
    public MyQuaternion Scale(float scale) {
        MyQuaternion clone = new MyQuaternion();
        clone.x = x * scale;
        clone.y = y * scale;
        clone.z = z * scale;
        clone.w = w * scale;
        return clone;
    }
    [ProtoBuf.ProtoMember(1)]
    public float w;
    [ProtoBuf.ProtoMember(2)]
    public float x;
    [ProtoBuf.ProtoMember(3)]
    public float y;
    [ProtoBuf.ProtoMember(4)]
    public float z;
    public static implicit operator Quaternion(MyQuaternion a)
    {
        return new Quaternion(a.x, a.y, a.z, a.w);
    }

    public static implicit operator MyQuaternion(Quaternion a)
    {
        return new MyQuaternion(a.w, a.x, a.y, a.z);
    }

    public static MyQuaternion operator +(MyQuaternion a, MyQuaternion b) {
        return new Quaternion(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
    }
}


