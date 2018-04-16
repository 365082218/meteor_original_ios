using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class FBXAnimationsFix : AssetPostprocessor
{
    public void OnPreprocessModel()
    {
        //当前正在导入的模型  
        ModelImporter modelImporter = (ModelImporter)assetImporter;

        AnimationClipConfig.init();

        foreach (AnimationClipConfig.modelST item in AnimationClipConfig.modelList)
        {
            //当前导入模型的路径包含我们modelST动作数据表中的模型名字，那就要对这个模型的动画进行切割  
            if (assetPath.Contains(item.ModelName))
            {
                modelImporter.animationType = ModelImporterAnimationType.Legacy;

                //modelImporter.splitAnimations = true;  
                modelImporter.generateAnimations = ModelImporterGenerateAnimations.GenerateAnimations;

                ModelImporterClipAnimation[] animations = new ModelImporterClipAnimation[item.clipSTs.Length];
                for (int i = 0; i < item.clipSTs.Length; i++)
                {
                    animations[i] = SetClipAnimation(item.clipSTs[i].name, item.clipSTs[i].firstFrame, item.clipSTs[i].lastFrame, item.clipSTs[i].isloop);
                }

                modelImporter.clipAnimations = animations;
            }
        }
    }

    ModelImporterClipAnimation SetClipAnimation(string _name, int _first, int _last, bool _isLoop)
    {
        ModelImporterClipAnimation tempClip = new ModelImporterClipAnimation();
        tempClip.name = _name;
        tempClip.firstFrame = _first;
        tempClip.lastFrame = _last;
        tempClip.loop = _isLoop;
        if (_isLoop)
            tempClip.wrapMode = WrapMode.Loop;
        else
            tempClip.wrapMode = WrapMode.Default;

        return tempClip;
    }
}



public static class AnimationClipConfig
{
    public static bool isInit = false;
    public static List<modelST> modelList = new List<modelST>();

    public static void init()
    {
        if (isInit)
            return;
        isInit = true;

        modelST tempModel = new modelST();
        tempModel.ModelName = "name";               //模型名字  
        tempModel.clipSTs = new clipST[]{
            new clipST("Step1" , 0, 20, false),
            new clipST("Step2" , 20, 40, false),
            new clipST("Step3" ,40, 70, false),
            new clipST("Step4" , 70, 90, false),
        };
        modelList.Add(tempModel);


    }

    #region ST
    public class clipST
    {
        public string name;
        public int firstFrame;
        public int lastFrame;
        public bool isloop;

        public clipST(string _n, int _f, int _l, bool _i)
        {
            name = _n;
            firstFrame = _f;
            lastFrame = _l;
            isloop = _i;
        }
    }

    public class modelST
    {
        public string ModelName;
        public clipST[] clipSTs;
    }
    #endregion
}