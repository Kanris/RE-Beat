using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityStandardAssets._2D;

[RequireComponent(typeof(Animator))]
public class MetalicGround : MonoBehaviour {

    #region private fields

    [Header("Item")]
    [SerializeField] private Item NeededItem; //required item to move on the metalic ground

    private BoxCollider2D m_Ground; //metalic ground
    private Animator m_Animator; //metalic ground animator

    #endregion

    #region private methods

    private void Start()
    {
        m_Ground = GetComponent<BoxCollider2D>(); //initialize ground

        m_Animator = GetComponent<Animator>(); //initialize animator
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player on metalic ground
        {
            if (PlayerStats.PlayerInventory.IsInBag(NeededItem.itemDescription.Name)) //if player has needed item
            {
                collision.transform.GetComponent<Platformer2DUserControl>().IsCanJump = false; //dont allow player to jump
                PlayAnimation("Active"); //change ground animation
            }
            else //if player havn't needed item
            {
                StartCoroutine(DisableGround(collision.gameObject.layer));
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player leave metalic ground
        {
            collision.transform.GetComponent<Platformer2DUserControl>().IsCanJump = true; //allow player to jump
            PlayAnimation("Inactive"); //change ground animation
        }
    }

    private void PlayAnimation(string name)
    {
        m_Animator.SetTrigger(name);
    }

    private IEnumerator DisableGround(int playerLayer)
    {
        Physics2D.IgnoreLayerCollision(playerLayer, gameObject.layer);

        yield return new WaitForSeconds(2f);

        Physics2D.IgnoreLayerCollision(playerLayer, gameObject.layer, false);
    }

    #endregion
}
