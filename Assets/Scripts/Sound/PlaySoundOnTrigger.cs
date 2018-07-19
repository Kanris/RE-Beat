using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnTrigger : MonoBehaviour {

    [SerializeField] private string Sound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SoundManager(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        SoundManager(collision, false);
    }

    private void SoundManager(Collider2D collision, bool PlaySound = true)
    {
        if (!string.IsNullOrEmpty(Sound))
        {
            if (collision.CompareTag("Player"))
            {
                if (PlaySound)
                {
                    AudioManager.Instance.Play(Sound, true);
                }
                else
                {
                    AudioManager.Instance.Stop(Sound, true);
                }
            }
        }
        else
        {
            Debug.LogError("PlaySoundOnTrigger.SoundManager: Sound name can't be empty");
        }

    }
}
