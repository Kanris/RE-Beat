using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoManagerLight : MonoBehaviour {

    public static InfoManagerLight Instance;

    [SerializeField] private GameObject m_InfoManagerGO;

    private Light m_InfoManagerLight;

    private void Awake()
    {
        Instance = this;
        m_InfoManagerLight = m_InfoManagerGO.GetComponent<Light>();
        m_InfoManagerGO.SetActive(false);
    }

    public void ChangeLight(int value)
    {
        m_InfoManagerGO.SetActive(true);

        switch (value)
        {
            case 0:
                //m_InfoManagerLight.color = m_InfoManagerLight.color.ChangeColor(0, 168, 243);
                m_InfoManagerLight.color = new Color(0, 168, 243);
                break;

            case 1:
                m_InfoManagerLight.color = new Color(184, 61, 186);
                break;

            case 2:
                m_InfoManagerLight.color = new Color(255, 127, 39);
                break;

            case 3:
                m_InfoManagerLight.color = new Color(14, 209, 69);
                break;

            default:
                m_InfoManagerGO.SetActive(false);
                break;
        }
    }
}
