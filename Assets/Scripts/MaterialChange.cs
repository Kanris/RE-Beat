using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MaterialChange : MonoBehaviour {

    #region private fields

    #region serialize fields

    [SerializeField] private Material DefaultMaterial;
    [SerializeField] private Material LightMaterial;
    [SerializeField] private GameObject LightOnObject;

    #endregion

    private SpriteRenderer m_SpriteRenderer;

    #endregion

    #region private methods

    private void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    #endregion

    #region public methods

    public void Change(bool isLight)
    {
        ChangeLight(isLight); //change light state
        ChangeMaterial(isLight); //change sprite material
    }

    public void ChangeMaterial(bool isLight)
    {
        if (isLight) //if there is light
        {
            m_SpriteRenderer.material = LightMaterial; //change to light material
            Debug.LogError(transform.name);
        }
        else //there is no light
        {
            m_SpriteRenderer.material = DefaultMaterial; //change to default material
        }
    }

    public void ChangeLight(bool isLight)
    {
        if (LightOnObject != null)
            LightOnObject.SetActive(isLight); //activate or deactivate light
    }

    #endregion
}
