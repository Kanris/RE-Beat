using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeFootStepsSound : MonoBehaviour {

    [SerializeField] private string Sound;
    private string PreviousSound;
    private bool isPlayerOnTrigger = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & !isPlayerOnTrigger)
        {
            PreviousSound = collision.gameObject.GetComponent<FootSound>().Sound;
            AudioManager.Instance.Stop(PreviousSound);
            collision.gameObject.GetComponent<FootSound>().Sound = Sound;

            isPlayerOnTrigger = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & isPlayerOnTrigger)
        {
            AudioManager.Instance.Stop(Sound);
            collision.gameObject.GetComponent<FootSound>().Sound = PreviousSound;

            isPlayerOnTrigger = false;
        }
    }
}
