
/*
 * UNITY BAKEMESH 残影实现
 * http://blog.csdn.net/bbvs1/article/details/10410775
 * http://www.xuebuyuan.com/1765183.html
 * 
 * 
 */
 //就是播放动画每一帧，然后把skinmesh上经过变换的网格bake到一个静态模型上
 //如果游戏不采用Animation动画，就不能这样做了.
using UnityEngine;
using System.Collections;

public class BakeMeshTest : MonoBehaviour
{
	void Start()
	{
		// Get the state for the clip we want to bake
		AnimationState clipState = m_animation[m_clipToBake];
		if (clipState == null)
		{
			Debug.LogError(string.Format("Unable to get clip '{0}'", m_clipToBake), this);
			return;
		}
		// Start playing the clip
		m_animation.Play(m_clipToBake, PlayMode.StopAll);
		// Set time to start
		clipState.time = 0.0f;
		// Calculate time between baked frames
		float deltaTime = clipState.length / (float)(m_numFramesToBake - 1);
		// Bake the frames
		for (int frameIndex = 0; frameIndex < m_numFramesToBake; ++frameIndex)
		{
			string frameName = string.Format("BakedFrame{0}", frameIndex);
			// Create a mesh to store this frame
			Mesh frameMesh = new Mesh();
			frameMesh.name = frameName;
			// Sample animation to get bones in the right place
			m_animation.Sample();
			// Bake the mesh
			m_skinnedMeshRenderer.BakeMesh(frameMesh);
			// Setup game object to show frame
			GameObject frameGO = new GameObject(frameName);
			
			frameGO.name = frameName;
			
			frameGO.transform.position = transform.position + new Vector3(frameIndex, 0.0f, 0.0f);
			
			// Setup mesh filter
			
			MeshFilter meshFilter = frameGO.AddComponent<MeshFilter>();
			
			meshFilter.mesh = frameMesh;
			
			// Setup mesh renderer
			
			MeshRenderer meshRenderer = frameGO.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterials = m_skinnedMeshRenderer.sharedMaterials;
			
			// Move to next frame time
			clipState.time += deltaTime;
			
		}
		
		// Stop playing
		
		m_animation.Stop();
		
	}
	
	[SerializeField]
	
	Animation m_animation; // Animation component used for baking
	
	[SerializeField]
	
	SkinnedMeshRenderer m_skinnedMeshRenderer; // Skinned mesh renderer used for baking
	
	[SerializeField]
	
	string m_clipToBake = "Idle"; // Name of the animation clip to bake
	[SerializeField] 
	int m_numFramesToBake = 30; // Number of frames to bake
	
}
