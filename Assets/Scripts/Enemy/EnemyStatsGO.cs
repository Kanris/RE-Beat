using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class EnemyStatsGO : MonoBehaviour {


    public Enemy EnemyStats;

    // Use this for initialization
    void Start () {

        InitializeStats();
    }

    public void InitializeStats()
    {
        EnemyStats.Initialize(gameObject, GetComponent<Animator>());
    }

    #region test methods

    [ContextMenu("CreateShieldOnEnemy")]
    public void CreateShield()
    {
        EnemyStats.CreateShield();
    }

    #endregion
}
