/*
	This script is placed in public domain. The author takes no responsibility for any possible harm.
	Contributed by Jonathan Czeck
*/
using UnityEngine;
using System.Collections;

public class LightningBoltParticle : MonoBehaviour
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
	
	//private Particle[] particles;
    private ParticleSystem.Particle[] particles;

    public bool IsStart = true;

    [Tooltip("水平停止半径(超出XZ水平距离则停止, 0无限制)")]
    public float XZRadius = 0;

    ParticleSystem _ps = null;
    ParticleSystem.Particle[] _arrParticles;
	
	void Start()
	{
		oneOverZigs = 1f / (float)zigs;
		//particleEmitter.emit = false;
        //particleEmitter.Emit(zigs);
        //particles = particleEmitter.particles;

        //particleSystem.enableEmission = false;

        //particleSystem.GetParticles(particles);//拿不出来的

        ////这样才能拿出来的
        //ParticleSystem _ps = this.GetComponent<ParticleSystem>();
        //int maxCount = _ps.maxParticles;
        //ParticleSystem.Particle[] _arrParticles = new ParticleSystem.Particle[maxCount];
        //int activeCount = _ps.GetParticles(_arrParticles);
        //for (int n = 0; n < activeCount; ++n)
        //{
        //    _arrParticles[n].position = new Vector3(0,n,0);
        //}
        //_ps.SetParticles(_arrParticles, _arrParticles.Length);

        _ps = this.GetComponent<ParticleSystem>();
        _arrParticles = new ParticleSystem.Particle[zigs];
        //int maxCount = _ps.maxParticles;
        //particles = new ParticleSystem.Particle[maxCount];
        //particleSystem.GetParticles(particles);

        //_ps.enableEmission = false;

	}
	
	void Update ()
	{
        if (Input.GetKeyUp(KeyCode.N))
        {
            IsStart = !IsStart;

            //particleEmitter.enabled = IsStart;
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


        ////这样才能拿出来的
        //ParticleSystem _ps = this.GetComponent<ParticleSystem>();
        //int maxCount = _ps.maxParticles;
        //ParticleSystem.Particle[] _arrParticles = new ParticleSystem.Particle[maxCount];
        int activeCount = _ps.GetParticles(_arrParticles);
        for (int i = 0; i < zigs; i++)
        {
            Vector3 position = Vector3.Lerp(transform.position, target.position + targetOffset, oneOverZigs * (float)i);

            Vector3 offset = new Vector3(noise.Noise(timex + position.x, timex + position.y, timex + position.z),
                                        noise.Noise(timey + position.x, timey + position.y, timey + position.z),
                                        noise.Noise(timez + position.x, timez + position.y, timez + position.z));
            position += (offset * scale * ((float)i * oneOverZigs));

            _arrParticles[i].position = position;
            //_arrParticles[i].rotation = 60;

            //_arrParticles[n].position = new Vector3(0, n, 0);
            Debug.Log(_arrParticles[i].position);
        }

        _ps.SetParticles(_arrParticles, _arrParticles.Length);


        return;


		
        //for (int i=0; i < particles.Length; i++)
        //{
        //    Vector3 position = Vector3.Lerp(transform.position, target.position + targetOffset, oneOverZigs * (float)i);

        //    Vector3 offset = new Vector3(noise.Noise(timex + position.x, timex + position.y, timex + position.z),
        //                                noise.Noise(timey + position.x, timey + position.y, timey + position.z),
        //                                noise.Noise(timez + position.x, timez + position.y, timez + position.z));
        //    position += (offset * scale * ((float)i * oneOverZigs));
			
        //    particles[i].position = position;
        //    //particles[i].color = Color.white;
        //    //particles[i].energy = 1f;
        //}
		
        ////particleEmitter.particles = particles;
        //_ps.SetParticles(particles, particles.Length);
		
        //if (particleEmitter.particleCount >= 2)
        //{
        //    if (startLight)
        //        startLight.transform.position = particles[0].position;
        //    if (endLight)
        //        endLight.transform.position = particles[particles.Length - 1].position;
        //}
	}	
}