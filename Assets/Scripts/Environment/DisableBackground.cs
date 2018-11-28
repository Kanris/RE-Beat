using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DisableBackground : MonoBehaviour {

    #region private fields

    #region serialize fields

    [Header("Animators that react on player")]
    [SerializeField] private Animator Background; //reference to the background image
    [SerializeField] private Animator Mist; //reference to the cave mist

    [Header("Objects that change material")]
    [SerializeField] private TilemapRenderer[] ChangeMaterialTilemap;
    [SerializeField] private SpriteRenderer[] ChangeObjectsMaterial;

    [Header("Materials to change")]
    [SerializeField] private Material m_DefaultMaterial;
    [SerializeField] private Material m_LightMaterial;

    #endregion

    public bool m_IsPlayerInCave; //is player in cave
    private bool m_IsFading; //is mist is fading

    #endregion

    #region private methods

    #region trigger

    private IEnumerator OnTriggerEnter2D(Collider2D collision)
    {
        ChangeMaterial(collision, true); //change object material

        if (collision.CompareTag("Player") & !m_IsPlayerInCave) //if player enter the cave
        {
            m_IsPlayerInCave = true; //player in cave

            StartCoroutine(FadeToBlack());

            yield return new WaitForSeconds(0.01f);

            ChangeCaveObjectsMaterial(true); //change object in cave material
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        ChangeMaterial(collision, false);

        if (collision.CompareTag("Player") & m_IsPlayerInCave) //if player leave cave
        {
            m_IsPlayerInCave = false;

            StartCoroutine(FadeToClear());

            yield return new WaitForSeconds(0.01f);

            ChangeCaveObjectsMaterial(false); //change cave objects material
        }
    }

    private IEnumerator FadeToClear()
    {
        yield return FadeTo("FadeIn"); //fade in mist
    }

    private IEnumerator FadeToBlack()
    {
        yield return FadeTo("FadeOut"); //remove mist
    }

    #endregion

    private IEnumerator FadeTo(string trigger)
    {
        m_IsFading = true; //fade in progress

        if (Mist != null) Mist.SetTrigger(trigger); //fade mist

        if (Background != null) Background.SetTrigger(trigger); //fade background

        while (m_IsFading) //continue to execute this method until animation is over
            yield return null;
    }

    private void ChangeMaterial(Collider2D collision, bool isEnter)
    {
        if (collision.CompareTag("Player") | collision.CompareTag("Enemy") | collision.CompareTag("Item")) //if item/enemy or player enter/leave the cave
        {
            ChangeObjectMaterial(collision, isEnter); //change material
        }
    }

    private void ChangeCaveObjectsMaterial(bool isEnter)
    {
        if (ChangeMaterialTilemap != null)
        {
            if (ChangeMaterialTilemap.Length > 0) //if there is cave items
            {
                var material = isEnter ? m_LightMaterial : m_DefaultMaterial; //is player enter the cave than apply light material or if player leave cave set default material

                //apply new material
                for (var index = 0; index < ChangeMaterialTilemap.Length; index++)
                {
                    if (ChangeMaterialTilemap[index] != null)
                        ChangeMaterialTilemap[index].material = material;
                }

                for (var index = 0; index < ChangeObjectsMaterial.Length; index++)
                {
                    if (ChangeObjectsMaterial[index] != null)
                        ChangeObjectsMaterial[index].material = material;
                }
            }
        }
    }

    private void ChangeObjectMaterial(Collider2D collision, bool isEnter)
    {
        var objectMaterialChange = collision.GetComponent<MaterialChange>();

        if (objectMaterialChange != null)
            objectMaterialChange.Change(isEnter); //change object material
    }

    #endregion

    #region public methods

    public void AnimationComplete()
    {
        m_IsFading = false;
    }

    #endregion
}
