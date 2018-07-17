using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAudio : MonoBehaviour {

    [SerializeField] private string SoundName;
    [SerializeField] private float FadeTime = 0.2f;
    [SerializeField] private float Increment = -0.005f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioManager.Instance.Play(SoundName, true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioManager.Instance.Stop(SoundName, true);
        }
    }


}
