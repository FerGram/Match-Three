using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public GameObject clearFXPrefab;
    public GameObject breakFXPrefab;
    public GameObject doubleBreakFXPrefab;
    public GameObject bombFXPrefab;

    public void ClearDotFXAt(int x, int y, int z = 0)
    {
        if (clearFXPrefab != null)
        {
            GameObject FX = Instantiate(clearFXPrefab, new Vector3(x, y, z), Quaternion.identity);
            ParticlePlayer particlePlayer = FX.GetComponent<ParticlePlayer>();
            if (particlePlayer != null)
            {
                particlePlayer.Play();  
            }
        }
    }
    public void BreakTileFXAt(int breakableValue, int x, int y, int z = 0)
    {
        GameObject FX = null;
        ParticlePlayer particlePlayer = null;
        if (breakableValue > 1)
        {
            if (doubleBreakFXPrefab != null)
            {
                FX = Instantiate(doubleBreakFXPrefab, new Vector3(x, y, z), Quaternion.identity);
            }
        }
        else
        {
            if (doubleBreakFXPrefab != null)
            {
                FX = Instantiate(breakFXPrefab, new Vector3(x, y, z), Quaternion.identity);
            }
        }
        if (FX != null)
        {
            particlePlayer = FX.GetComponent<ParticlePlayer>();
            if (particlePlayer != null)
            {
                particlePlayer.Play();
            }
        }
    }
    public void BombFXAt(int x, int y, int z = 0)
    {
        if (bombFXPrefab != null)
        {
            GameObject bombFX = Instantiate(bombFXPrefab, new Vector3(x, y, z), Quaternion.identity);
            ParticlePlayer particlePlayer = bombFX.GetComponent<ParticlePlayer>();

            if (particlePlayer != null){
                particlePlayer.Play();
            }
        }
    }
}
