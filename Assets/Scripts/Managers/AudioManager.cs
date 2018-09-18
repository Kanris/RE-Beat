using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

    [System.Serializable]
    public class Audio
    {
        #region public fields

        public string Name;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 0.5f;
        [Range(0.5f, 1.5f)] public float Pitch = 1f;
        [Range(0f, 0.5f)] public float VolumeOffset = 0.1f;
        [Range(0f, 0.5f)] public float PitchOffset = 0.1f;
        public AudioType m_AudioType;
        public bool Loop = false;

        #endregion

        #region private fields

        private AudioSource m_AudioSource;
        private bool m_PlayerReturn = true;
        private bool Unload = false;

        #endregion

        #region public methods

        public void SetSource(AudioSource source, AudioMixerGroup mixerGroup)
        {
            m_AudioSource = source;
            m_AudioSource.clip = Clip;
            m_AudioSource.playOnAwake = false;
            ChangeMusicSettings();
            m_AudioSource.outputAudioMixerGroup = mixerGroup;

            if (!m_AudioSource.clip.preloadAudioData)
            {
                Unload = true;
                m_AudioSource.clip.UnloadAudioData();
            }
        }

        public void PlaySound()
        {
            if (m_AudioSource != null)
            {
                if (!m_AudioSource.isPlaying)
                {
                    if (Unload) m_AudioSource.clip.LoadAudioData();

                    ChangeMusicSettings();
                    m_AudioSource.Play();
                }
            }
        }

        public void StopSound()
        {
            if (m_AudioSource != null)
            {
                m_AudioSource.Stop();

                if (Unload) m_AudioSource.clip.UnloadAudioData();
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

        #endregion

        #region private methods

        private void ChangeMusicSettings()
        {
            m_AudioSource.volume = Volume * (1 + Random.Range(-VolumeOffset / 2f, VolumeOffset / 2f));
            m_AudioSource.pitch = Pitch * (1 + Random.Range(-PitchOffset / 2f, PitchOffset / 2f)); ;
            m_AudioSource.loop = Loop;
        }

        #endregion
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

            InitializeAudioPlaylist();
        }

    }

    #endregion

    #region public fields

    public enum AudioType { Environment, Music }

    public Audio[] AudioArray;

    #endregion

    #region private fields

    [SerializeField] private AudioMixerGroup MusicAudioMixer;
    [SerializeField] private AudioMixerGroup EnvironmentAudioMixer;

    private string m_BackgroundMusic;

    #endregion

    #region private methods

    private void InitializeAudioPlaylist()
    {
        for (int index = 0; index < AudioArray.Length; index++)
        {
            var audioSource = new GameObject("AudioSource_" + index + "_" + AudioArray[index]);
            audioSource.transform.SetParent(transform);

            AudioArray[index].SetSource(audioSource.AddComponent<AudioSource>(), 
                GetAudioMixerGroup(AudioArray[index].m_AudioType));
        }
    }

    private AudioMixerGroup GetAudioMixerGroup(AudioType type)
    {
        var mixerGroup = MusicAudioMixer;

        if (type == AudioType.Environment)
            mixerGroup = EnvironmentAudioMixer;

        return mixerGroup;
    }

    private Audio GetAudioFromArray(string name)
    {
        Audio returnAudio = null;

        if (!string.IsNullOrEmpty(name))
            returnAudio = AudioArray.FirstOrDefault(x => x.Name == name);

        return returnAudio;
    }

    #endregion

    #region public methods

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

    public void SetBackgroundMusic(string name)
    {
        if (m_BackgroundMusic != name)
        {
            if (!string.IsNullOrEmpty(m_BackgroundMusic)) //stop current background music
            {
                var sound = GetAudioFromArray(m_BackgroundMusic);

                if (sound != null)
                {
                    StartCoroutine(sound.FadeOut());
                }
            }

            if (!string.IsNullOrEmpty(name)) //play new background music
            {
                m_BackgroundMusic = name;
                var sound = GetAudioFromArray(m_BackgroundMusic);

                if (sound != null)
                {
                    for (int index = 0; index < AudioArray.Length; index++)
                    {
                        AudioArray[index].StopSound();
                    }

                    StartCoroutine(sound.FadeIn());
                }
                else
                {
                    Debug.LogError("AudioManager.SetBackgroundMusic: can't find audio with name - " + name);
                }
            }
            else
                Debug.LogError("AudioManager.SetBackgroundMusic: can't change background music, because name is null or empty");
        }
    }

    #endregion
}
