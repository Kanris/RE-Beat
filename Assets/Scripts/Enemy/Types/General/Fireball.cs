using UnityEngine;

public class Fireball : MonoBehaviour {

    [HideInInspector] public Vector3 Direction; //where is fireball moving
    [HideInInspector] public float DestroyTime; //after what amount of time fireball will be destroyed

    [SerializeField] private int DamageAmount = 2; //fireball damage amount
    [SerializeField] private float Speed = 2.5f; //fireball movement speed
    [SerializeField] private bool isNeedRotation = true; 
    [SerializeField] private float DestroyDelay = 0.2f; //delay to play destroy animation
    [SerializeField] private LayerMask m_LayerMask; //what to damage

    private Animator m_Animator; //fireball animation
    private bool isDestroying = false; //is fireball destroying

	// Use this for initialization
	void Start () {

        InitializeDirection();

        InitializeAnimator();

        DestroyTime = Time.time + 5f;
    }

    private void InitializeDirection()
    {
        if (isNeedRotation)
        {
            if (Direction == Vector3.left)
                transform.Rotate(0, 0, 180);

            else if (Direction == Vector3.up)
                transform.Rotate(0, 0, 90);

            else if (Direction == Vector3.down)
                transform.Rotate(0, 0, 270);

            else if (Direction == new Vector3(1, 1, 0))
                transform.Rotate(0, 0, 50);

            else if (Direction == new Vector3(-1, 1, 0))
                transform.Rotate(0, 0, 140);

            else if (Direction == new Vector3(-1, -1, 0))
                transform.Rotate(0, 0, 230);

            else if (Direction == new Vector3(1, -1, 0))
                transform.Rotate(0, 0, 310);
        }
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>(); //get fireball animator

        if (m_Animator == null)
        {
            Debug.LogError("Fireball.InitializeAnimator: Can't find component - Animator on Gameobject");
        }
    }

    private void Update()
    {
        //is player not on scene and he is not returning on return point
        var isPlayerOnScene =
            (GameMaster.Instance.m_Player?.transform.GetChild(0).gameObject.activeSelf ?? false)
            && ((GameMaster.Instance.m_Player?.name.Contains("Player") ?? false)
            || (GameMaster.Instance.m_Player?.name.Contains("Companion") ?? false));

        //if fireball life time is over or player is not on scene and fireball is not destroying
        if ((Time.time >= DestroyTime || !isPlayerOnScene) && !isDestroying)
            DestroyFireball(); //destroy fireball
    }

    // Update is called once per frame
    void FixedUpdate() {

        if (!isDestroying)
            transform.position += Direction * Time.fixedDeltaTime * Speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((m_LayerMask & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer) & !isDestroying)
        {
            collision.gameObject.GetComponent<Player>().playerStats.HitPlayer(DamageAmount);
        }

        DestroyFireball();
    }

    private void DestroyFireball()
    {
        isDestroying = true;

        m_Animator.SetBool("isCollide", isDestroying);

        Destroy(gameObject, DestroyDelay);
    }
}
