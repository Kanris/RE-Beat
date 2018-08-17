using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChange : MonoBehaviour {

    [SerializeField] private Material DefaultMaterial;
    [SerializeField] private Material LightMaterial;
    [SerializeField] private GameObject LightOnObject;

    private SpriteRenderer m_SpriteRenderer;

    #region Initialize

    private void Awake()
    {
        InitializeSpriteRenderer();
    }

    private void InitializeSpriteRenderer()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();

        if (m_SpriteRenderer == null)
        {
            Debug.LogError("Player.InitializeSpriteRenderer: Can't find sprite renderer on player gameobject");
        }
    }

    #endregion

    public void Change(bool isLight)
    {
        ChangeLight(isLight);
        ChangeMaterial(isLight);
    }

    public void ChangeMaterial(bool isLight)
    {
        if (m_SpriteRenderer != null)
        {
            if (isLight)
            {
                m_SpriteRenderer.material = LightMaterial;
            }
            else
            {
                m_SpriteRenderer.material = DefaultMaterial;
            }
        }
    }

    public void ChangeLight(bool isLight)
    {
        if (LightOnObject != null)
            LightOnObject.SetActive(isLight);
    }
}
