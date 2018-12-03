using UnityEngine;
using System.Collections;
//using Cinemachine.Editor;

public class ObjectAppearOnTrigger : MonoBehaviour {

    #region private fields

    public enum ReactOn { Player, PlayerBullet }

    [SerializeField] private ReactOn m_React;
    [SerializeField] private GameObject[] m_AppearObject; //object to show
    [SerializeField] private GameObject m_ShowOnDestroy; //show on destroy some object

    [Header("Animation (optional)")]
    [SerializeField] private Animator m_AppearObjectAnimator;
    [SerializeField] private string m_AnimationToPlay;

    [Header("Destroy conditions")]
    [SerializeField] private bool m_DestroyOnTrigger;

    [SerializeField] private bool m_ActiveAfterDestroy;

    [Header("Camera")]
    [SerializeField] private bool m_IsCameraControl;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera m_VirtualCamera;
    [SerializeField] private Transform m_Center;
    [SerializeField, Range(1f, 20f)] private float m_CamSize = 5f;
    
    private bool m_IsQuitting; //is application is closing
    private Transform m_ObjectToFollow;
    private float m_DefaultCamSize;

    public bool m_IsPlayerTrigger;

    #endregion

    #region private methods

    #region Initialize

    private void Start()
    {
        m_IsQuitting = false;

        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting;
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting;
    }

    private void Update()
    {
        if (m_IsPlayerTrigger)
        {
            if (Vector3.Distance(m_VirtualCamera.gameObject.transform.position, m_Center.position) != 0)
                m_VirtualCamera.gameObject.transform.position =
                    Vector3.MoveTowards(m_VirtualCamera.gameObject.transform.position, m_Center.position, 20f * Time.deltaTime);
            else
                m_IsPlayerTrigger = false;
        }
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(m_React.ToString())) //if player in collision
        {
            AppearObject(); //show object
            DestroyThisTrigger(); //destroy this object

            if (m_IsCameraControl)
            {
                if (m_VirtualCamera.Follow != null)
                {
                    m_IsPlayerTrigger = true;

                    m_ObjectToFollow = m_VirtualCamera.Follow;
                    m_DefaultCamSize = m_VirtualCamera.m_Lens.OrthographicSize;

                    m_VirtualCamera.Follow = null;
                    //m_VirtualCamera.gameObject.transform.position = m_Center.position;
                    //m_VirtualCamera.m_Lens.OrthographicSize = m_CamSize;

                    StartCoroutine(SmoothChangeCameraSize());   
                }
            }
        }
    }

    private IEnumerator SmoothChangeCameraSize()
    {
        var value = m_CamSize / 10;

        while (m_VirtualCamera.m_Lens.OrthographicSize < m_CamSize)
        {
            m_VirtualCamera.m_Lens.OrthographicSize += value;
            yield return new WaitForSeconds(.1f);
        }
    }

    private void AppearObject()
    {
        if (m_AppearObject != null)
        {
            foreach (var item in m_AppearObject)
                item.SetActive(true);

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

            if (m_IsCameraControl & m_ObjectToFollow != null)
            {
                m_VirtualCamera.Follow = m_ObjectToFollow;
                m_VirtualCamera.m_Lens.OrthographicSize = m_DefaultCamSize;
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
