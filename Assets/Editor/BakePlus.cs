using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class BakePlus : ScriptableWizard
{

	[MenuItem("Build/Lighting bake")]
	static void Bake()
	{
		LightmapEditorSettings.maxAtlasHeight = 512;
		LightmapEditorSettings.maxAtlasWidth = 512;
		Lightmapping.Clear();
		Lightmapping.Bake();
	}

}
