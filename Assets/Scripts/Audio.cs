using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New Audio", menuName = "AudioClipForManager")]
[System.Serializable]
public class Audio : ScriptableObject
{
    #region public fields


    public enum AudioType { Environment, Music }

    public AudioClip Clip;
    [Range(0f, 1f)] public float Volume = 1f;
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

    public void SetSource(AudioSource source)
    {
        m_AudioSource = source;
        m_AudioSource.clip = Clip;
        m_AudioSource.playOnAwake = false;
        ChangeMusicSettings();

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

    public IEnumerator FadeOut(float fadeTime = 0.05f, float increment = 0.02f)
    {
        m_PlayerReturn = false;

        while (m_AudioSource.volume != 0f & !m_PlayerReturn)
        {
            m_AudioSource.volume -= increment;
            yield return new WaitForSeconds(fadeTime);
        }

        if (!m_PlayerReturn)
        {
            m_AudioSource.Stop();
        }
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

    public void SetVolume(float value)
    {
        Volume = value;
        m_AudioSource.volume = value;
    }

    public static implicit operator string(Audio audio)
    {
        return audio.name;
    }

    #endregion

    #region private methods

    private void ChangeMusicSettings()
    {
        m_AudioSource.volume = Volume;
        m_AudioSource.pitch = Pitch;
        m_AudioSource.loop = Loop;
    }

    #endregion
}
