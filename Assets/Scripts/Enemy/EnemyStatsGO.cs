using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class EnemyStatsGO : MonoBehaviour {

    public Enemy EnemyStats;

    [SerializeField] private bool DontResurect;

    // Use this for initialization
    void Start () {

        InitializeStats();

        if (DontResurect)
            EnemyStats.OnObjectDeath += SaveState;
    }

    public void InitializeStats()
    {
        EnemyStats.Initialize(gameObject, GetComponent<Animator>());
    }

    public void SaveState()
    {
        GameMaster.Instance.SaveState(transform.name, 0, GameMaster.RecreateType.Object);
    }
}
