using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeFootStepsSound : MonoBehaviour {

    [SerializeField] private string Sound;
    private string PreviousSound;
    private bool isPlayerOnTrigger = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ManageSound(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ManageSound(collision, true);
        isPlayerOnTrigger = false;
    }

    private void ManageSound(Collider2D collision, bool stop = false)
    {
        if (collision.CompareTag("Player") & !isPlayerOnTrigger)
        {
            isPlayerOnTrigger = true;

            if (stop)
            {
                AudioManager.Instance.Stop(Sound);
                collision.gameObject.GetComponent<FootSound>().Sound = PreviousSound;
            }
            else
            {
                PreviousSound = collision.gameObject.GetComponent<FootSound>().Sound;
                AudioManager.Instance.Stop(PreviousSound);
                collision.gameObject.GetComponent<FootSound>().Sound = Sound;
            }
        }
    }
}
