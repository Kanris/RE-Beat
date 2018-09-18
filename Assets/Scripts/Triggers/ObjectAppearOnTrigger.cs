using UnityEngine;

public class ObjectAppearOnTrigger : MonoBehaviour {

    #region private fields

    [SerializeField] private GameObject ObjectToAppear; //object to show
    [SerializeField] private GameObject ShowOnDestroy; //show on destroy some object
    [SerializeField] private bool DestroyOnTrigger; 

    private bool m_IsQuitting; //is application is closing

    #endregion

    #region private methods

    #region Initialize

    private void Start()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting;
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting;
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player in collision
        {
            AppearObject(); //show object
            DestroyThisTrigger(); //destroy this object
        }
    }

    private void AppearObject()
    {
        if (ObjectToAppear != null)
        {
            ObjectToAppear.SetActive(true);
        }
    }

    private void DestroyThisTrigger()
    {
        if (DestroyOnTrigger)
        {
            GameMaster.Instance.SaveState(gameObject.name, 0, GameMaster.RecreateType.Object);
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting)
        {
            if (ShowOnDestroy != null)
            {
                ShowOnDestroy.SetActive(true);
            }
        }
    }

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true);
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }

    #endregion
}
