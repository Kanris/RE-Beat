using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;

public class MapManager : MonoBehaviour {

    #region public fields

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnMapOpen;

    #endregion

    #region private fields

    [SerializeField] private TextMeshProUGUI m_LocationName;
    [SerializeField] private GameObject m_UI;
    [SerializeField] private RawImage m_Minimap;
    [SerializeField] private Image[] imagesToTransparent;

    private bool m_IsMoving;
    private bool m_IsTransparent;
    private bool m_IsTransparing;
    private Animator m_PlayerAnimator;
    private float m_UpdateSearchTime;
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

    private void Start()
    {
        m_UI.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
		
        if (Input.GetKeyDown(KeyCode.M))
        {
            m_UI.SetActive(!m_UI.activeSelf);

            m_LocationName.text = GameMaster.Instance.SceneName;

            if (OnMapOpen != null)
                OnMapOpen(m_UI.activeSelf);
        }

        if (m_UI.activeSelf & m_PlayerAnimator != null)
        {
            float h = m_PlayerAnimator.GetFloat("Speed");
            float v = m_PlayerAnimator.GetFloat("vSpeed");

            if ((h != 0f | v != 0f) & !m_IsMoving)
            {
                m_IsTransparent = true;
                m_IsMoving = true;
            }
            else if ((h == 0f & v == 0f) & m_IsMoving)
            {
                m_IsTransparent = true;
                m_IsMoving = false;
            }

        }

        if (m_IsTransparent & !m_IsTransparing)
        {
            StartCoroutine(TransparentImages(m_IsMoving));
        }

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
        m_IsTransparing = true;

        yield return new WaitForSeconds(0.5f);

        if (value == m_IsMoving)
        {
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
}
