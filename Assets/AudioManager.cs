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
        public bool IsLoop = false;

        private AudioSource m_AudioSource;

        public void SetSource(AudioSource source)
        {
            m_AudioSource = source;
            Clip = m_AudioSource.clip;
            m_AudioSource.playOnAwake = false;
        }

        public void PlaySound()
        {
            if (m_AudioSource != null)
            {
                ChangeMusicSettings();
                m_AudioSource.Play();
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

        public override string ToString()
        {
            return Name;
        }

        public static implicit operator string(Audio audio)
        {
            return audio.Name;
        }

        private void ChangeMusicSettings()
        {
            m_AudioSource.volume = Volume * (1 + Random.Range(-VolumeOffset / 2f, VolumeOffset / 2f));
            m_AudioSource.pitch = Pitch * (1 + Random.Range(-PitchOffset / 2f, PitchOffset / 2f)); ;
            m_AudioSource.loop = IsLoop;
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

    // Use this for initialization
    void Start () {

        for (int index = 0; index < AudioArray.Length; index++)
        {
            var audioSource = new GameObject("AudioSource_" + index + "_" + AudioArray[index]);
            audioSource.transform.SetParent(transform);
            AudioArray[index].SetSource(audioSource.AddComponent<AudioSource>());
        }

	}

    public void Play(string name)
    {
        var sound = AudioArray.First(x => x == name);

        if (sound != null)
        {
            sound.PlaySound();
        }
        else
        {
            Debug.LogError("AudioManager: can't find audio with name - " + name);
        }
    }
}
