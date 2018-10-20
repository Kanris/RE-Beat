using UnityEngine;

public class ObjectAppearOnTrigger : MonoBehaviour {

    #region private fields

    public enum ReactOn { Player, PlayerBullet }

    [SerializeField] private ReactOn m_React;
    [SerializeField] private GameObject m_AppearObject; //object to show
    [SerializeField] private GameObject m_ShowOnDestroy; //show on destroy some object

    [Header("Animation (optional)")]
    [SerializeField] private Animator m_AppearObjectAnimator;
    [SerializeField] private string m_AnimationToPlay;

    [Header("Destroy conditions")]
    [SerializeField] private bool m_DestroyOnTrigger;

    [SerializeField] private bool m_ActiveAfterDestroy;

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
        if (m_AppearObject != null)
        {
            m_AppearObject.SetActive(true);
            PlayAnimation();
        }
    }

    private void DestroyThisTrigger()
    {
        if (m_DestroyOnTrigger)
        {
            GameMaster.Instance.SaveState(gameObject.name, 0, GameMaster.RecreateType.Object);
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting)
        {
            if (m_ShowOnDestroy != null)
            {
                m_ShowOnDestroy.SetActive(true);
            }
            
            if (m_ActiveAfterDestroy)
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

    private void PlayAnimation()
    {
        if (m_AppearObjectAnimator != null)
        {
            m_AppearObjectAnimator.SetBool(m_AnimationToPlay, true);
        }
    }

    #endregion
}
