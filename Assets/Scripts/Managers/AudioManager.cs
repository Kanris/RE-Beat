using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

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

    private AudioMixerGroup GetAudioMixerGroup(Audio.AudioType type)
    {
        var mixerGroup = MusicAudioMixer;

        if (type == Audio.AudioType.Environment)
            mixerGroup = EnvironmentAudioMixer;

        return mixerGroup;
    }

    private Audio GetAudioFromArray(string name)
    {
        Audio returnAudio = null;

        if (!string.IsNullOrEmpty(name))
            returnAudio = AudioArray.FirstOrDefault(x => x.name == name);

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
                for (int index = 0; index < AudioArray.Length; index++)
                {
                    if (AudioArray[index].name != m_BackgroundMusic)
                    {
                        AudioArray[index].StopSound();
                    }
                }

                m_BackgroundMusic = name;
                var sound = GetAudioFromArray(m_BackgroundMusic);

                StartCoroutine(sound.FadeIn());
            }
            else
                Debug.LogError("AudioManager.SetBackgroundMusic: can't change background music, because name is null or empty");
        }
    }

    #endregion
}
