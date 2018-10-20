using UnityEngine;

public class ObjectAppearOnTrigger : MonoBehaviour {

    #region private fields

    public enum ReactOn { Player, PlayerBullet }

    [SerializeField] private ReactOn m_React;
    [SerializeField] private GameObject ObjectToAppear; //object to show
    [SerializeField] private GameObject ShowOnDestroy; //show on destroy some object

    [Header("Destroy conditions")]
    [SerializeField] private bool DestroyOnTrigger;

    [SerializeField] private bool LeftActiveAfterShow;

    private bool m_IsQuitting; //is application is closing

    #endregion

    #region private methods

    #region Initialize

    private void Start()
    {
        m_IsQuitting = false;

        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting;
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting;
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(m_React.ToString())) //if player in collision
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
            
            if (LeftActiveAfterShow)
            {
                AppearObject();
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
