using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

public class ScrapStone : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField, Range(6, 15)] private int m_ScrapsAmount = 8; //scraps amount per hit
    [SerializeField, Range(20, 40)] private int m_ScrapsOnDestroy = 30; //scraps amount when destroy scrapstone

    [Header("Additional")]
    [SerializeField] private GameObject m_ScrapEffect; //scrap gameobject

    private Camera2DFollow m_Camera;

    private void Start()
    {
        m_Camera = Camera.main.GetComponent<Camera2DFollow>(); //get main camera script
        var enemyStats = GetComponent<EnemyStatsGO>(); //get enemy stats on gameobject

        enemyStats.EnemyStats.OnEnemyTakeDamageValue += AddSmallScrapsAmount; //on hit give small amount of scraps
        enemyStats.EnemyStats.OnObjectDeath += AddBigScrapsAmount; //on destroy give big amount of scraps
    }

    private void AddSmallScrapsAmount(float value)
    {
        // 16, 25 50 - damage values
        var divider = value <= 20 ? 3 : value <= 30 ? 2 : 1; //change scraps amount base on the damage value
        var randomAmount = Random.Range(m_ScrapsAmount - 5, m_ScrapsAmount); //get scraps

        CreateScrapEffect(randomAmount / divider); //create scraps gameobject
    }

    private void AddBigScrapsAmount()
    {
        CreateScrapEffect(m_ScrapsOnDestroy); //create scraps gameobject with big amount of scraps
    }

    private void CreateScrapEffect(int scrapAmount)
    {
        m_Camera.Shake(.05f, .05f); //shake camera
        var instantScrapEffect = Instantiate(m_ScrapEffect); //create scraps effect on scene
        instantScrapEffect.transform.position = transform.position; //place created scraps effect on this gameobject

        var player = GameMaster.Instance.m_Player.transform.GetChild(0).transform; //get player transform

        instantScrapEffect.GetComponent<ScrapObject>().SetTarget(player, scrapAmount); //move scraps to player
    }
}
