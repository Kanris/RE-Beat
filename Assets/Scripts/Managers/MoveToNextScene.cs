using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToNextScene : MonoBehaviour {

    public static MoveToNextScene Instance;

    private void Awake()
    {
        Instance = this;
    }

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate IsMoveToNextScene;

    [SerializeField] private string NextScene;
    [SerializeField] private string NextScenename;
    [SerializeField] private Vector2 SpawnPosition;

    private bool isPlayerNear;
    private bool isDestroyingItem;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !isPlayerNear)
        {
            isPlayerNear = true;
            if (!string.IsNullOrEmpty(NextScene))
            {
                if (IsMoveToNextScene != null)
                    IsMoveToNextScene(true);

                LoadSceneManager.Instance.StartCoroutine(
                    LoadSceneManager.Instance.LoadWithFade(NextScene, NextScenename, SpawnPosition));
            }
            else
                Debug.LogError("MoveToNextScene.OnTriggerEnter2D: NextScene variable is not initialized.");
        }
        else if (collision.CompareTag("Item") & !isDestroyingItem)
        {
            isDestroyingItem = true;
            Destroy(collision);
            isDestroyingItem = false;
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
