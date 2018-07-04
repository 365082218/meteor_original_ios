//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Localization manager is able to parse localization information from text assets.
/// Although a singleton, you will generally not access this class as such. Instead
/// you should implement "void Localize (Localization loc)" functions in your classes.
/// Take a look at UILocalize to see how it's used.
/// </summary>

[AddComponentMenu("NGUI/Internal/Localization")]
public class Localization : MonoBehaviour
{
	static Localization mInstance;

	/// <summary>
	/// The instance of the localization class. Will create it if one isn't already around.
	/// </summary>

	static public Localization instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = Object.FindObjectOfType(typeof(Localization)) as Localization;

				if (mInstance == null)
				{
					GameObject go = new GameObject("_Localization");
					DontDestroyOnLoad(go);
					mInstance = go.AddComponent<Localization>();
				}
			}
			return mInstance;
		}
	}

	/// <summary>
	/// Language the localization manager will start with.
	/// </summary>

	public string startingLanguage = "English";

	/// <summary>
	/// Available list of languages.
	/// </summary>

	public TextAsset[] languages;

	Dictionary<string, string> mDictionary = new Dictionary<string, string>();
	string mLanguage;

	/// <summary>
	/// Name of the currently active language.
	/// </summary>

	public string currentLanguage
	{
		get
		{
			return mLanguage;
		}
		set
		{
			if (mLanguage != value)
			{
				startingLanguage = value;

				if (!string.IsNullOrEmpty(value))
				{
					// Check the referenced assets first
					if (languages != null)
					{
						for (int i = 0, imax = languages.Length; i < imax; ++i)
						{
							TextAsset asset = languages[i];

							if (asset != null && asset.name == value)
							{
								Load(asset);
								return;
							}
						}
					}

					// Not a referenced asset -- try to load it dynamically
					TextAsset txt = Resources.Load(value, typeof(TextAsset)) as TextAsset;

					if (txt != null)
					{
						Load(txt);
						return;
					}
				}

				// Either the language is null, or it wasn't found
				mDictionary.Clear();
				PlayerPrefs.DeleteKey("Language");
			}
		}
	}

	/// <summary>
	/// Determine the starting language.
	/// </summary>

	void Awake ()
	{
		if (mInstance == null)
		{
			mInstance = this;
			DontDestroyOnLoad(gameObject);

			currentLanguage = PlayerPrefs.GetString("Language", startingLanguage);

			if (string.IsNullOrEmpty(mLanguage) && (languages != null && languages.Length > 0))
			{
				currentLanguage = languages[0].name;
			}
		}
		else Destroy(gameObject);
	}

	/// <summary>
	/// Oddly enough... sometimes if there is no OnEnable function in Localization, it can get the Awake call after UILocalize's OnEnable.
	/// </summary>

	void OnEnable () { if (mInstance == null) mInstance = this; }

	/// <summary>
	/// Remove the instance reference.
	/// </summary>

	void OnDestroy () { if (mInstance == this) mInstance = null; }

	/// <summary>
	/// Load the specified asset and activate the localization.
	/// </summary>

	void Load (TextAsset asset)
	{
		mLanguage = asset.name;
		PlayerPrefs.SetString("Language", mLanguage);
		ByteReader reader = new ByteReader(asset);
		mDictionary = reader.ReadDictionary();
		UIRoot.Broadcast("OnLocalize", this);
	}

	/// <summary>
	/// Localize the specified value.
	/// </summary>

	public string Get (string key)
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return key;
#endif
		string val;
#if UNITY_IPHONE || UNITY_ANDROID
		if (mDictionary.TryGetValue(key + " Mobile", out val)) return val;
#endif

#if UNITY_EDITOR
		if (mDictionary.TryGetValue(key, out val)) return val;
		Debug.LogWarning("Localization key not found: '" + key + "'");
		return key;
#else
		return (mDictionary.TryGetValue(key, out val)) ? val : key;
#endif
	}

	/// <summary>
	/// Localize the specified value.
	/// </summary>

	static public string Localize (string key) { return (instance != null) ? instance.Get(key) : key; }
}
