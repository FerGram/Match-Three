using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public AudioClip[] musicClips;
    public AudioClip[] winClips;
    public AudioClip[] loseClips;
    public AudioClip[] bonusClips;

    public float lowPitch = 0.95f;
    public float highPitch = 1.05f;

    [Range(0, 1)] public float musicVolume = 0.5f;
    [Range(0, 1)] public float fxVolume = 0.5f;
    [Range(0, 1)] public float winAndLoseVolume = 0.5f;

    void Start()
    {
        PlayRandomMusic();
    }

    public AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip != null)
        {
            GameObject go = new GameObject("SoundFX " + clip.name);
            go.transform.position = position;

            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;

            float randomPitch = Random.Range(lowPitch, highPitch);
            source.pitch = randomPitch;
            source.volume = volume;

            source.Play();
            Destroy(go, clip.length);

            return source;
        }
        return null;
    }
    public AudioSource PlayRandomClip(AudioClip[] clips, Vector3 position, float volume = 1f)
    {
        if (clips != null && clips.Length != 0)
        {
            int randomInt = Random.Range(0, clips.Length);
            if (clips[randomInt] != null)
            {
                AudioSource source = PlayClipAtPoint(clips[randomInt], position, volume);
                return source;
            }
        }
        return null;
    }

    public void PlayRandomMusic()
    {
        PlayRandomClip(musicClips, Vector3.zero, musicVolume);
    }
    public void PlayRandomWinSound()
    {
        PlayRandomClip(winClips, Vector3.zero, winAndLoseVolume);
    }
    public void PlayRandomLoseSound()
    {
        PlayRandomClip(loseClips, Vector3.zero, winAndLoseVolume * 0.5f);
    }
    public void PlayRandomBonusSound()
    {
        PlayRandomClip(bonusClips, Vector3.zero, fxVolume);
    }
}
