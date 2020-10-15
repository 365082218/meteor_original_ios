using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserPref:Pref<UserPref>{
}

//当日24时失效
[ProtoContract]
public class Pref<T>:Singleton<T> {
    string file;
    public Dictionary<string, Cookie> cookies = new Dictionary<string, Cookie>();
    public int GetInt(string key, int defaultValue) {
        return (int)Get(key, defaultValue);
    }

    object Get(string key, object defaultValue) {
        if (cookies.ContainsKey(key))
            return cookies[key].value;
        return defaultValue;
    }

    public bool GetBool(string key, bool defaultValue) {
        return (bool)Get(key, defaultValue);
    }

    public string GetString(string key, string defaultValue) {
        return (string)Get(key, defaultValue);
    }

    public float GetFloat(string key, float defaultValue) {
        return (float)Get(key, defaultValue);
    }

    void Set(string key, object value) {
        if (cookies.ContainsKey(key)) {
            cookies[key].value = value;
            cookies[key].start = DateTime.Today.ToFileTime();
            cookies[key].expired = DateTime.Today.AddDays(1).ToFileTime();
        }
    }

    public void SetFloat(string key, float v) {
        Set(key, v);
    }
    
    public void SetInt(string key, int v) {
        Set(key, v);
    }

    public void SetBool(string key, bool v) {
        Set(key, v);
    }

    public void SetString(string key, string v) {
        Set(key, v);
    }

    //检查所有的cookie,把失效的删除.
    public void CookieExpire() {
        List<string> keys = new List<string>();
        foreach (var each in cookies) {
            keys.Add(each.Key);
        }
        for (int i = 0; i < keys.Count; i++) {
            if (cookies[keys[i]].expired < DateTime.Today.ToFileTime())
                cookies.Remove(keys[i]);
        }
    }

    public void Save() {
        System.IO.FileStream fs = null;
        try {
            fs = new System.IO.FileStream(file, System.IO.FileMode.OpenOrCreate);
            Serializer.Serialize(fs, this);
            fs.Close();
        } catch (Exception exp) {
            Debug.LogError("save user cookie error:" + exp.Message);
            if (fs != null) {
                fs.Close();
                fs = null;
            }
        }
    }

    public virtual void Load(string path) {
        file = path;
        System.IO.FileStream fs = null;
        try {
            if (System.IO.File.Exists(path)) {
                fs = new System.IO.FileStream(Main.Ins.userPath, System.IO.FileMode.Open);
                cookies = Serializer.Deserialize<Dictionary<string, Cookie>>(fs);
                fs.Close();
            }
        } catch (Exception exp) {
            Debug.LogError("load user cookie error:" + exp.Message);
            if (fs != null) {
                fs.Close();
                fs = null;
            }
        }
    }
}

//可按过期设定的临时配置
[ProtoContract]
public class Cookie {
    public long expired;//过期时长
    public long start;//保存日期
    public object value;
}