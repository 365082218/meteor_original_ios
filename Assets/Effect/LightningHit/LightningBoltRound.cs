/*
	This script is placed in public domain. The author takes no responsibility for any possible harm.
	Contributed by Jonathan Czeck
*/
using UnityEngine;
using System.Collections;

public class LightningBoltRound : MonoBehaviour
{
	public Transform target;

    //此参数应该是和Ellipsoid Particle Emitter中的 Min Emission 和 Max Emission 匹配的
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

    //旋转改变的角度
    public float changeAngle = 3.6f;
    private float angle = 0;
    public float r = 5;

	
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
			//Vector3 position = Vector3.Lerp(transform.position, target.position, oneOverZigs * (float)i);

            Vector3 center = transform.position;
            //GameObject cube = (GameObject)Instantiate(circleModel);
            float hudu = (angle / 180) * Mathf.PI;
            float xx = center.x + r * Mathf.Cos(hudu);
            float zz = center.y + r * Mathf.Sin(hudu);
            //cube.transform.position = new Vector3(xx, yy, 0);
            //cube.transform.LookAt(center);
            Vector3 position = new Vector3(xx, center.y, zz);
            angle += changeAngle;


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