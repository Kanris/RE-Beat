using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapManager : MonoBehaviour {

    #region public fields

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnMapOpen;

    #endregion

    #region private fields

    #region serialize fields

    [SerializeField] private TextMeshProUGUI m_LocationName; //scene name
    [SerializeField] private GameObject m_UI; //map manager ui
    [SerializeField] private RawImage m_Minimap; //mini map
    [SerializeField] private Image[] imagesToTransparent; //image to transparent when player moves

    #endregion

    private bool m_IsMoving; //is player moving
    private bool m_IsTransparent; //is images transparent
    private bool m_IsTransparing; //is transparent in progress
    private Animator m_PlayerAnimator; //player animator
    private float m_UpdateSearchTime; //search player time

    #endregion

    #region singleton

    public static MapManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    #endregion

    #region private methods

    private void Start()
    {
        m_UI.SetActive(false);
    }

    // Update is called once per frame
    private void Update () {

        //if map button pressed
        if (Input.GetKeyDown(KeyCode.M)) 
        {
            m_UI.SetActive(!m_UI.activeSelf); //show/hide map ui

            if (m_UI.activeSelf & m_LocationName.text != GameMaster.Instance.SceneName) //change location name
                m_LocationName.text = GameMaster.Instance.SceneName;

            if (OnMapOpen != null)
                OnMapOpen(m_UI.activeSelf); //notify that map is close or open
        }

        //if map is open and we have reference to the player animator
        if (m_UI.activeSelf & m_PlayerAnimator != null) 
        {
            //get player current movement
            float h = m_PlayerAnimator.GetFloat("Speed");
            float v = m_PlayerAnimator.GetFloat("vSpeed");

            if ((h != 0f | v != 0f) & !m_IsMoving) //if player is moving
            {
                m_IsTransparent = true; //need to transparent 
                m_IsMoving = true;
            }
            else if ((h == 0f & v == 0f) & m_IsMoving) //if is not moving but game think it is
            {
                m_IsTransparent = true; //stop transparent
                m_IsMoving = false;
            }

        }

        //transparent image
        if (m_IsTransparent & !m_IsTransparing) 
        {
            StartCoroutine(TransparentImages(m_IsMoving));
        }

        //search for player is there is no reference to the player animator
        if (m_PlayerAnimator == null)
        {
            if (m_UpdateSearchTime < Time.time)
            {
                m_UpdateSearchTime = Time.time + 1f;

                var player = GameObject.FindWithTag("Player");

                if (player != null)
                {
                    m_PlayerAnimator = player.GetComponent<Animator>();
                }
            }
        }
	}

    private IEnumerator TransparentImages(bool value)
    {
        m_IsTransparing = true; //notify that transparents starts

        yield return new WaitForSeconds(0.5f);

        //if player still moving
        if (value == m_IsMoving) 
        {

            //transparent all images
            var alphaValue = 0.5f / 10f;

            if (value)
                alphaValue *= -1;

            for (int iteration = 0; iteration < 10; iteration++)
            {
                var color = new Color(m_Minimap.color.r, m_Minimap.color.g, m_Minimap.color.b, m_Minimap.color.a + alphaValue);

                imagesToTransparent[0].color = imagesToTransparent[1].color = m_Minimap.color = color;

                yield return new WaitForSeconds(0.01f);
            }
        }

        m_IsTransparent = false;
        m_IsTransparing = false;
    }

    #endregion
}
