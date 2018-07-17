using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    [System.Serializable]
    public class Audio
    {
        public string Name;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 0.5f;
        [Range(0.5f, 1.5f)] public float Pitch = 1f;
        [Range(0f, 0.5f)] public float VolumeOffset = 0.1f;
        [Range(0f, 0.5f)] public float PitchOffset = 0.1f;
        public bool Loop = false;

        private AudioSource m_AudioSource;
        private bool m_PlayerReturn = true;

        public void SetSource(AudioSource source)
        {
            m_AudioSource = source;
            m_AudioSource.clip = Clip;
            m_AudioSource.playOnAwake = false;

            ChangeMusicSettings();
        }

        public void PlaySound()
        {
            if (m_AudioSource != null)
            {
                if (!m_AudioSource.isPlaying)
                {
                    ChangeMusicSettings();
                    m_AudioSource.Play();
                }
            }
            else
            {
                Debug.LogError("Audio: m_AudioSource is equals to null");
            }
        }

        public void StopSound()
        {
            if (m_AudioSource != null)
            {
                m_AudioSource.Stop();
            }
            else
            {
                Debug.LogError("Audio: m_AudioSource is equals to null");
            }
        }

        public IEnumerator FadeOut(float fadeTime = 0.5f, float increment = 0.02f)
        {
            m_PlayerReturn = false;
            
            while (m_AudioSource.volume != 0f & !m_PlayerReturn)
            {
                m_AudioSource.volume -= increment;
                yield return new WaitForSeconds(fadeTime);
            }

            if (!m_PlayerReturn)
                m_AudioSource.Stop();
        }

        public IEnumerator FadeIn(float fadeTime = 0.5f, float increment = 0.02f)
        {
            m_PlayerReturn = true;

            m_AudioSource.Play();
            ChangeMusicSettings();

            while (m_AudioSource.volume < Volume)
            {
                m_AudioSource.volume += increment;
                yield return new WaitForSeconds(fadeTime);
            }

        }
       

        public static implicit operator string(Audio audio)
        {
            return audio.Name;
        }

        private void ChangeMusicSettings()
        {
            m_AudioSource.volume = Volume * (1 + Random.Range(-VolumeOffset / 2f, VolumeOffset / 2f));
            m_AudioSource.pitch = Pitch * (1 + Random.Range(-PitchOffset / 2f, PitchOffset / 2f)); ;
            m_AudioSource.loop = Loop;
        }
    }

    #region Singleton
    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    #endregion

    public Audio[] AudioArray;

    private void Start()
    {
        InitializeAudioPlaylist();
    }

    private void InitializeAudioPlaylist()
    {
        for (int index = 0; index < AudioArray.Length; index++)
        {
            var audioSource = new GameObject("AudioSource_" + index + "_" + AudioArray[index]);
            audioSource.transform.SetParent(transform);
            AudioArray[index].SetSource(audioSource.AddComponent<AudioSource>());
        }
    }

    public void Play(string name, bool PlayFadeSound = false)
    {
        var sound = GetAudioFromArray(name);

        if (sound != null)
        {
            if (PlayFadeSound)
                StartCoroutine(sound.FadeIn());
            else
                sound.PlaySound();
        }
        else
        {
            Debug.LogError("AudioManager.Play: can't find audio with name - " + name);
        }
    }

    public void Stop(string name, bool PlayFadeSound = false)
    {
        var sound = GetAudioFromArray(name);

        if (sound != null)
        {
            if (PlayFadeSound)
                StartCoroutine(sound.FadeOut());
            else
                sound.StopSound();
        }
        else
        {
            Debug.LogError("AudioManager.Stop: can't find audio with name - " + name);
        }
    }

    private Audio GetAudioFromArray(string name)
    {
        Audio returnAudio = null;
        
        if (!string.IsNullOrEmpty(name))
            returnAudio = AudioArray.First(x => x == name);

        return returnAudio;
    }
}
