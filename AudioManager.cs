using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour, IGameManager
{
    public AudioSource playOneShotSource;
    public AudioSource allMoneyCollected;
    public AudioSource levelFailedSource;
    [Space]
    [Space]
    public AudioSource fight1MusicSource;
    public AudioSource fight2MusicSource;
    public AudioSource UltraPowerSource;
    [Space]
    [Space]
    public AudioSource ambientSource;

    [SerializeField] private AudioClip[] _ambientClips;

    public ManagerStatus status { get; private set; }

    private float _musicVolume;

    public void Startup()
    {
        Debug.Log("AudioManager starting");

        status = ManagerStatus.Started;
    }

    public void PlayAmbientSound()
    {
        int randomAmbient = Random.Range(0, _ambientClips.Length);
        ambientSource.clip = _ambientClips[randomAmbient];
        ambientSource.Play();
    }

    public void PlaySound(AudioClip clip)
    {
        playOneShotSource.clip = clip;
        playOneShotSource.Play();
    }

    public float SoundVolume
    {
        get { return AudioListener.volume; }
        set { AudioListener.volume = value; }
    }

    public bool SoundMute
    {
        get { return AudioListener.pause; }
        set { AudioListener.pause = value; }
    }

    public float MusicVolume
    {
        get
        {
            return _musicVolume;
        }
        set
        {
            _musicVolume = value;
            fight1MusicSource.volume = _musicVolume;
            fight2MusicSource.volume= _musicVolume;
            ambientSource.volume = _musicVolume;
            allMoneyCollected.volume = _musicVolume;
            UltraPowerSource.volume = _musicVolume;
            levelFailedSource.volume = _musicVolume;
        }
    }
    
    public bool MusicMute
    {
        get
        {
            if (fight1MusicSource != null)
            {
                return fight1MusicSource.mute;
            }

            return false;
        }
        set
        {
            if (fight1MusicSource != null)
            {
                fight1MusicSource.mute = value;
                fight2MusicSource.mute = value;
                ambientSource.mute = value;
                allMoneyCollected.mute = value;
                UltraPowerSource.mute = value;
                levelFailedSource.mute = value;
            }
        }

    }

    public void StopMusic()
    {
        fight1MusicSource.Stop();
        fight2MusicSource.Stop();
        ambientSource.Stop();
        allMoneyCollected.Stop();
        UltraPowerSource.Stop();
        levelFailedSource.Stop();
    }

    public void PlayRandomFightMusic()
    {
        int randomMusic = Random.Range(0, 2); // 0 ��� 1.
            
        if (randomMusic == 0) 
            fight1MusicSource.Play();
        
        else fight2MusicSource.Play();
    }


    public IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        if (audioSource == null || !audioSource.isPlaying)
        {
            yield break;
        }

        float startVolume = audioSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null; // ����� �� ���� �����������, � ���������.
        }

        audioSource.volume = 0;
        audioSource.Stop();
    }

    public IEnumerator FadeIn(AudioSource audioSource, float duration)
    {
        if (audioSource == null)
            yield break;

        float targetVolume = 1f;
        audioSource.volume = 0;
        audioSource.Play();

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, targetVolume, t / duration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }

    
}