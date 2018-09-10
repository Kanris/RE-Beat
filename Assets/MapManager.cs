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
    [SerializeField] private Image[] imagesToTransparent; 
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

        if (m_UI.activeSelf)
        {
            float h = CrossPlatformInputManager.GetAxis("Horizontal");

            if (h != 0f)
            {
                TransparentImages(true);
            }
        }

	}

    private void TransparentImages(bool value)
    {
        foreach (var item in imagesToTransparent)
        {
            var newColor = item.color;
            newColor.a = 0f;

            item.color = newColor;
        }
    }
}
