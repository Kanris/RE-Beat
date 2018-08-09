using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityStandardAssets._2D;

[RequireComponent(typeof(TilemapCollider2D))]
public class MetalicGround : MonoBehaviour {

    public string NeededItem = "Magnetic Boots";

    private TilemapCollider2D m_Ground;
    private Animator m_Animator;

    private void Start()
    {
        InitializeGround();

        InitializeAnimator();
    }

    #region Initialize

    private void InitializeGround()
    {
        m_Ground = GetComponent<TilemapCollider2D>();

        if (m_Ground == null)
        {
            Debug.LogError("MetalicGround.InitializeGround: Can't find CompositeCollider2D on gameobject");
        }
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("MetalicGround.InitializeAnimator: Can't find Animator on gameobject");
        }
    }

    #endregion  

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (PlayerStats.PlayerInventory.IsInBag(NeededItem))
            {
                collision.transform.GetComponent<Platformer2DUserControl>().IsCanJump = false;
                PlayAnimation("Active");
            }
            else
                StartCoroutine(DisableGround());
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.transform.GetComponent<Platformer2DUserControl>().IsCanJump = true;
            PlayAnimation("Inactive");
        }
    }

    private void PlayAnimation(string name)
    {
        m_Animator.SetTrigger(name);
    }

    private IEnumerator DisableGround()
    {
        m_Ground.enabled = false;

        yield return new WaitForSeconds(1f);

        m_Ground.enabled = true;
    }
}
