/*
	This script is placed in public domain. The author takes no responsibility for any possible harm.
	Contributed by Jonathan Czeck
*/
using UnityEngine;
using System.Collections;

public class LightningBolt : MonoBehaviour
{
	public Transform target;
    public Vector3 targetOffset = Vector3.zero;

	public int zigs = 100;
	public float speed = 1f;
	public float scale = 1f;
	public Light startLight;
	public Light endLight;
	
	Perlin noise;
	float oneOverZigs;
	
	private Particle[] particles;

    public bool IsStart = true;

    [Tooltip("水平停止半径(超出XZ水平距离则停止, 0无限制)")]
    public float XZRadius = 0;
	
	void Start()
	{
		oneOverZigs = 1f / (float)zigs;
		GetComponent<ParticleEmitter>().emit = false;

		GetComponent<ParticleEmitter>().Emit(zigs);
		particles = GetComponent<ParticleEmitter>().particles;
	}
	
	void Update ()
	{
        if (Input.GetKeyUp(KeyCode.N))
        {
            IsStart = !IsStart;

            GetComponent<ParticleEmitter>().enabled = IsStart;
            this.GetComponent<ParticleRenderer>().enabled = IsStart;
            if (startLight)
				startLight.enabled = IsStart;
            if (endLight)
                endLight.enabled = IsStart;
        }

        if (!IsStart)
            return;

		if (noise == null)
			noise = new Perlin();
			
		float timex = Time.time * speed * 0.1365143f;
		float timey = Time.time * speed * 1.21688f;
		float timez = Time.time * speed * 2.5564f;
		
		for (int i=0; i < particles.Length; i++)
		{
            Vector3 position = Vector3.Lerp(transform.position, target.position + targetOffset, oneOverZigs * (float)i);

			Vector3 offset = new Vector3(noise.Noise(timex + position.x, timex + position.y, timex + position.z),
										noise.Noise(timey + position.x, timey + position.y, timey + position.z),
										noise.Noise(timez + position.x, timez + position.y, timez + position.z));
			position += (offset * scale * ((float)i * oneOverZigs));
			
			particles[i].position = position;
			particles[i].color = Color.white;
			particles[i].energy = 1f;
		}
		
		GetComponent<ParticleEmitter>().particles = particles;
		
		if (GetComponent<ParticleEmitter>().particleCount >= 2)
		{
			if (startLight)
				startLight.transform.position = particles[0].position;
			if (endLight)
				endLight.transform.position = particles[particles.Length - 1].position;
		}
	}	
}