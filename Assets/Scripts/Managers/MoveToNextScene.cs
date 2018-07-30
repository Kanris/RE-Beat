using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToNextScene : MonoBehaviour {

    [SerializeField] private string NextScene;

    private bool isPlayerNear;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !isPlayerNear)
        {
            isPlayerNear = true;
            if (!string.IsNullOrEmpty(NextScene))
            {
                PickupBox.isQuitting = true;
                LoadSceneManager.Instance.LoadWithFade(NextScene);
            }
            else
                Debug.LogError("MoveToNextScene.OnTriggerEnter2D: NextScene variable is not initialized.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & isPlayerNear)
        {
            isPlayerNear = false;
        }
    }

}
