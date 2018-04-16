using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * 例子系统挂上此脚本不受游戏TimeScale=0或其他速度影响
 * 
 * 只需把脚本挂在根节点即可
 * 
 * 
 */

public class ldaParticleManualSpeed : MonoBehaviour
{

    /** 在Time.timeScale>0情况下，可动态控制粒子速度 */
    public float _particleSpeed = 1.0f;//Lindean

    private double lastTime;
    private List<ParticleSystem> particles = new List<ParticleSystem>();

    private void Awake()
    {
        if (particles == null)
            particles = new List<ParticleSystem>();
        particles.Clear();
        ParticleSystem[] psys = GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in psys)
            particles.Add(ps);
    }

    // Use this for initialization
    void Start()
    {
        lastTime = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    void Update()
    {
        //游戏速度相同不做处理
        if (Time.timeScale == _particleSpeed)
            return;

        float deltaTime = Time.realtimeSinceStartup - (float)lastTime;

        foreach (ParticleSystem ps in particles)
            ps.Simulate(deltaTime * _particleSpeed, true, false); //Lindean

        lastTime = Time.realtimeSinceStartup;
    }


}
