using System.Collections;
using UnityEngine;
using UnityStandardAssets._2D;

public class RevivePlayer : MonoBehaviour {

    [Header("Objects to spawn")]
    [SerializeField] private GameObject m_PlayerToRevive; //player prefab to respawn
    [SerializeField] private GameObject m_Companion; //companion follow prefab to respawn

    [Header("Effects")]
    [SerializeField] private GameObject m_ReviveParticles; //particles effect that will be played when companion reach RevivePlayer

    private bool m_IsReviving; //indicates that player is reviving
    private GameObject m_PlayerThatInteract; //companion prefab that interact with reviveplayer

    private int m_ScrapAmount; //scrap amount that will be return after player reviving

    private void Start()
    {
        m_ScrapAmount = PlayerStats.Scrap; //get current scrap amount
        PlayerStats.Scrap = -PlayerStats.Scrap; //remove all scrap from the player

        UIManager.Instance.EnableCompanionUI(); //change ui
    }

    private IEnumerator Revive()
    {
        var materialForPlayer = StopInvisibleAbility(); //reverse invisible ability effect and get current player material for instantiated prefabs

        yield return Camera.main.GetComponent<Camera2DFollow>().PlayReviveEffect(); //play revive effect

        Destroy(m_PlayerThatInteract.transform.parent.gameObject); //destroy player that interacted with RevivePlayer

        //create player on scene
        InstantiatePlayer(materialForPlayer);

        //create follow companion on scene
        InstantiateCompanion(materialForPlayer);

        //remove RevivePlayer chip from scene
        Destroy(gameObject);
    }

    private void InstantiatePlayer(Material materialForPlayer)
    {
        //instantiate player on scene
        GameMaster.Instance.m_Player = Instantiate(m_PlayerToRevive, transform.position, transform.rotation);
        GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<SpriteRenderer>().material = materialForPlayer;

        //instantiate revive particles and destroy them after 5 secs
        Destroy(
            Instantiate(m_ReviveParticles, transform.position, Quaternion.identity), 5f);

        //return scraps to the player
        PlayerStats.Scrap = m_ScrapAmount;

        //change current ui
        UIManager.Instance.EnableRegularUI();

        //remove 1 revive from player
        UseRevive();

        //indicate that player is not dead
        GameMaster.Instance.IsPlayerDead = false;
    }

    private void InstantiateCompanion(Material materialForPlayer)
    {
        //instantiate follow companion
        Instantiate(m_Companion, transform.position, transform.rotation).transform.GetChild(0)
            .GetComponent<SpriteRenderer>().material = materialForPlayer;
    }

    private Material StopInvisibleAbility()
    {
        //get current player material
        var materialForPlayer = m_PlayerThatInteract.GetComponent<SpriteRenderer>().material;

        //remove invisible ability effect
        materialForPlayer.color = materialForPlayer.color.ChangeColor(a: 1f);
        Physics2D.IgnoreLayerCollision(8, 13, false); //ignore all enemies

        return materialForPlayer;
    }

    private void UseRevive()
    {
        UIManager.Instance.RemoveRevive(PlayerStats.m_ReviveCount);
        PlayerStats.m_ReviveCount -= 1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !m_IsReviving) //if player is in trigger and player is not reviving
        {
            m_IsReviving = true; //indicates that revive is start

            m_PlayerThatInteract = collision.gameObject; //get player gameobject

            m_PlayerThatInteract.GetComponent<Platformer2DUserControl>().enabled = false; //remove control from player
            m_PlayerThatInteract.GetComponent<Rigidbody2D>().velocity = Vector2.zero; //stop moving

            StartCoroutine(Revive()); //start reviving
        }
    }
}
