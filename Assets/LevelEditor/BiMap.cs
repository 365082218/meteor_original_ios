using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BiMap<T, U>
{
	static Dictionary<T, U> value = new Dictionary<T, U>();
	static Dictionary<U, T> valueR = new Dictionary<U, T>();

	public void Add(T k, U v)
	{
		if (!value.ContainsKey(k))
			value.Add(k, v);
		if (!valueR.ContainsKey(v))
			valueR.Add(v, k);
	}

	public U GetValue(T k)
	{
		if (value.ContainsKey(k))
			return value[k];
		return default(U);
	}

	public T GetValue(U v)
	{
		if (valueR.ContainsKey(v))
			return valueR[v];
		return default(T);
	}

	public void Clear()
	{
		value.Clear();
		valueR.Clear();
	}

	public void Remove(T key)
	{
		U valuevalue = default(U);
		if (value.ContainsKey(key))
		{
			valuevalue = value[key];
			value.Remove(key);
		}
		if (valueR.ContainsKey(valuevalue))
			valueR.Remove(valuevalue);
	}

	public void Remove(U key)
	{
		T valuevalue = default(T);
		if (valueR.ContainsKey(key))
		{
			valuevalue = valueR[key];
			valueR.Remove(key);
		}
		if (value.ContainsKey(valuevalue))
			value.Remove(valuevalue);
	}
}
