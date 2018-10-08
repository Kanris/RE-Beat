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

    [ContextMenu("CreateShieldOnEnemy")]
    public void CreateShield()
    {
        EnemyStats.CreateShield();
    }
}
