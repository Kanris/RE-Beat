using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DisableBackground : MonoBehaviour {

    #region private fields

    #region serialize fields

    [SerializeField] private Animator BackgroundAnimator; //reference to the background image
    [SerializeField] private Animator MistAnimator; //reference to the cave mist
    [SerializeField] private TilemapRenderer[] ChangeMaterialTilemap;
    [SerializeField] private Material m_DefaultMaterial;
    [SerializeField] private Material m_LightMaterial;

    #endregion

    private bool m_PlayerInCave; //is player in cave
    private bool m_IsFading; //is mist is fading

    #endregion

    #region private methods

    #region trigger

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_PlayerInCave) //if player enter the cave
        {
            PlayerEnterCave(); //fade out mist
        }

        ChangeMaterial(collision, true); //change object material
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_PlayerInCave) //if player leave cave
        {
            var isNeedWaiting = false; //fade mist without waiting

            if (GameMaster.Instance.isPlayerDead) //if player died in cave
                isNeedWaiting = true; //fade mist with waiting

            StartCoroutine(PlayerLeaveCave(isNeedWaiting)); //fade in mist
        }

        ChangeMaterial(collision, false);
    }

    #endregion

    private void PlayerEnterCave()
    {
        m_PlayerInCave = true; //player in cave

        StartCoroutine(FadeToClear()); //remove mist

        ChangeCaveObjectsMaterial(true); //change object in cave material
    }

    private IEnumerator PlayerLeaveCave(bool isNeedWaiting)
    {
        m_PlayerInCave = false; //player leave cave
         
        if (isNeedWaiting) //if need delay before fade in mist
            yield return new WaitForSeconds(1.5f);

        if (!m_PlayerInCave) //if player is not in cave
        {
            StartCoroutine(FadeToBlack()); //fade in mist
            ChangeCaveObjectsMaterial(false); //change cave objects material
        }
    }

    private IEnumerator FadeToClear()
    {
        yield return FadeTo("FadeOut"); //fade mist and background to clear
    }

    private IEnumerator FadeToBlack()
    {
        yield return FadeTo("FadeIn"); //fade mist and background to black
    }

    private IEnumerator FadeTo(string trigger)
    {
        m_IsFading = true; //fade in progress

        if (MistAnimator != null) MistAnimator.SetTrigger(trigger); //fade mist

        if (BackgroundAnimator != null) BackgroundAnimator.SetTrigger(trigger); //fade background

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
                    ChangeMaterialTilemap[index].material = material;
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
