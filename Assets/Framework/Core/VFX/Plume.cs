using System;
using UnityEngine;

public class Plume : MonoBehaviour
{
    public Animator anim;
    public ParticleSystem particles;
    public float timer;

    public void OnEnable()
    {
        GameMan.inst.camShaker.ShakeOnce(30f, 4f, 0f, 0.85f);
        anim.Play("Plume", 0, 0f);
        particles.Play();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0) Destroy(gameObject);
    }
}
