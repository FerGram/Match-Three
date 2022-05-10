using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    public float lifetime = 1f;
    public ParticleSystem[] allParticles;

    void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();
        Destroy(gameObject, lifetime);
    }

    public void Play()
    {
        foreach (ParticleSystem ps in allParticles)
        {
            ps.Stop();
            ps.Play();
        }
    }
}
