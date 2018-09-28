using System.Collections;
using UnityEngine;
using UnityEditor;
using Pathfinding;

[CustomEditor(typeof(DroneKamikaze)), CanEditMultipleObjects]
public class DroneScriptEditor : Editor
{
    public SerializedProperty
             drone_Type,
             whatIsEnemy_Prop,
             trailMaterial_Prop,
             bulletPrefab_Prop,
             deathParticles_Prop,
             detectionTrigger_Prop,
             deathDetonationTimer_Prop,
             damageAmount_Prop,
             speed_Prop,
             updateRate_Prop,
             minPosition_Prop,
             maxPosition_Prop,
             health_Prop,
             attackSpeed_Prop;

    private void OnEnable()
    {
        // Setup the SerializedProperties
        drone_Type = serializedObject.FindProperty("m_DroneType");
        whatIsEnemy_Prop = serializedObject.FindProperty("WhatIsEnemy");
        trailMaterial_Prop = serializedObject.FindProperty("TrailMaterial");
        bulletPrefab_Prop = serializedObject.FindProperty("BulletTrailPrefab");
        deathParticles_Prop = serializedObject.FindProperty("DeathParticles");
        detectionTrigger_Prop = serializedObject.FindProperty("DetectionTrigger");
        deathDetonationTimer_Prop = serializedObject.FindProperty("DeathDetonationTimer");
        damageAmount_Prop = serializedObject.FindProperty("DamageAmount");
        speed_Prop = serializedObject.FindProperty("Speed");

        updateRate_Prop = serializedObject.FindProperty("UpdateRate");
        minPosition_Prop = serializedObject.FindProperty("m_MinPosition");
        maxPosition_Prop = serializedObject.FindProperty("m_MaxPosition");
        health_Prop = serializedObject.FindProperty("Health");
        attackSpeed_Prop = serializedObject.FindProperty("AttackSpeed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(drone_Type);

        var droneType = (DroneKamikaze.DroneType)drone_Type.enumValueIndex;

        if (droneType == DroneKamikaze.DroneType.Shooter)
            InitializeShooterGUI();

        EditorGUILayout.ObjectField(deathParticles_Prop, new GUIContent("Death Particle"));
        EditorGUILayout.Slider(deathDetonationTimer_Prop, 0f, 10f, new GUIContent("Death Detonation Timer"));
        EditorGUILayout.IntSlider(damageAmount_Prop, 0, 10, new GUIContent("Damage Amount"));
        EditorGUILayout.IntSlider(health_Prop, 0, 10, new GUIContent("Health"));

        serializedObject.ApplyModifiedProperties();
        
    }
   

    private void InitializeShooterGUI()
    {
        EditorGUILayout.ObjectField(trailMaterial_Prop, new GUIContent("TrailMaterial"));
        EditorGUILayout.ObjectField(bulletPrefab_Prop, new GUIContent("BulletPrefab"));
        EditorGUILayout.ObjectField(detectionTrigger_Prop, new GUIContent("Detection Trigger"));
        EditorGUILayout.Slider(speed_Prop, 100f, 1000f, new GUIContent("Speed"));
        EditorGUILayout.Slider(updateRate_Prop, 1f, 10f, new GUIContent("Update Rate"));
        EditorGUILayout.Slider(minPosition_Prop, 0f, -5f, new GUIContent("Min Position"));
        EditorGUILayout.Slider(maxPosition_Prop, 10f, 5f, new GUIContent("Max Position"));
        EditorGUILayout.Slider(attackSpeed_Prop, 1f, 5f, new GUIContent("Attack Speed"));
    }
}

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Seeker))]
public class DroneKamikaze : MonoBehaviour {

    public enum DroneType { Kamikaze, Shooter };
    public DroneType m_DroneType; //drone type

    [SerializeField] private LayerMask WhatIsEnemy;
    [SerializeField] private Material TrailMaterial;
    [SerializeField] private GameObject BulletTrailPrefab;
    [SerializeField] private GameObject DeathParticles; //particles that shows after drone destroy
    [SerializeField] private PlayerInTrigger DetectionTrigger; //trigger that detect player

    [SerializeField, Range(0f, 10f)] private float DeathDetonationTimer = 2f; //time before destroying drone
    [SerializeField, Range(0, 10)] private int DamageAmount = 1; //damage to the player
    [SerializeField, Range(100f, 1000f)] private float Speed = 300f; //drone speed
    [SerializeField, Range(1f, 10f)] private float UpdateRate = 3f; //next point update rate
    [SerializeField, Range(0f, -5f)] private float m_MinPosition = -3f; //min position around the drone
    [SerializeField, Range(0f, 5f)] private float m_MaxPosition = 3f; //max poisition around the drone
    [SerializeField, Range(1, 5)] private int Health = 1; //drone health
    [SerializeField, Range(1f, 5f)] private float AttackSpeed = 2f;

    private Rigidbody2D m_Rigidbody;
    private Seeker m_Seeker;
    private Path m_Path;
    private Transform Target; //player
    private bool m_PathIsEnded = false; //path is reached
    private float m_NextWaypointDistance = 0.5f; 
    private int m_CurrentWaypoint = 0;

    private bool m_IsDestroying = false; //is drone going to blow up
    private bool m_IsPlayerInAttackRange = false; //is drone chasing player
    private bool m_IsAttacking = false;
    private float m_AttackCooldown;

    #region initialize

    // Use this for initialization
    private void Start () {

        InitializeComponents(); //initialize rigidbody and seeker

        if (m_DroneType == DroneType.Kamikaze) //initialize kamikaze drone type
            InitializeKamikaze();
        else //initialize shooter drone type
            InitializeShooter();
    }

    private void InitializeShooter()
    {
        StartCoroutine(PatrolingInRandomDirection()); //start random movement
        DetectionTrigger.OnPlayerInTrigger += PlayerInTrigger; //player in range detection
    }

    private void InitializeKamikaze()
    {
        //initialize random direction
        var randX = Random.Range(0, 2);
        var randY = Random.Range(0, 2);

        m_Rigidbody.velocity = new Vector2(randX == 0 ? -2f : 2f, randY == 0 ? -2f : 2f);
    }

    private void InitializeComponents()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Seeker = GetComponent<Seeker>();
    }

    #endregion

    private void FixedUpdate()
    {
        if (m_DroneType == DroneType.Kamikaze) //if drone type - kamikaze
        {
            //move in random direction
            m_Rigidbody.velocity = 
                new Vector2(Mathf.Clamp(m_Rigidbody.velocity.x, -5f, 5f), Mathf.Clamp(m_Rigidbody.velocity.y, -5f, 5f));
        }
        else if (m_DroneType == DroneType.Shooter) //if drone - shooter
        {
            if (m_IsAttacking & !m_IsDestroying) //player in range and drone is not destroying
            {
                if (m_AttackCooldown < Time.time) //drone can attack
                {
                    m_AttackCooldown = Time.time + AttackSpeed; //next available attack time
                    StartCoroutine(Shoot()); //shoot at player
                }
            }
            else //drone has to patrol the are
                MoveInDirection();
        }
    }

    #region collision/trigger detections

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & !m_IsDestroying)
        {
            collision.transform.GetComponent<Player>().playerStats.TakeDamage(DamageAmount);

            if (m_DroneType == DroneType.Kamikaze) StartCoroutine( DestroyDrone() );
        }

        if (collision.gameObject.layer == 14 & m_IsDestroying) //object layer - ground
        {
            StartCoroutine( DestroyDrone(DeathDetonationTimer) );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttackRange") & !m_IsDestroying)
        {
            Health--;

            if (Health <= 0)
            {
                m_IsDestroying = true;
                m_Rigidbody.sharedMaterial = null;
                m_Rigidbody.gravityScale = 3f;
            }
        }
    }

    #endregion

    private IEnumerator DestroyDrone(float waitTimeBeforeDestroy = 0f)
    {
        yield return new WaitForSeconds(waitTimeBeforeDestroy);

        var destroyParticles = Instantiate(DeathParticles, transform.position, Quaternion.identity);
        Destroy(destroyParticles, 1f);

        var hit2D = Physics2D.OverlapCircle(transform.position, 2, WhatIsEnemy);

        if (hit2D != null)
            hit2D.GetComponent<Player>().playerStats.TakeDamage(DamageAmount);

        Destroy(gameObject.transform.parent.gameObject);
    }

    private void FindPlayer()
    {
        if (Target == null)
        {
            Target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void PlayerInTrigger(bool value)
    {
        m_IsPlayerInAttackRange = value;

        if (!m_IsDestroying)
        {
            if (m_IsPlayerInAttackRange)
            {
                FindPlayer();
                //InitializeChasing();
                StopAllCoroutines();
                m_IsAttacking = true;
            }
            else
            {
                Target = null;
                m_IsAttacking = false;
                StartCoroutine(PatrolingInRandomDirection());
            }
        }
    }

    #region shooting

    private IEnumerator Shoot()
    {
        if (Target != null)
        {
            var whereToShoot = new Vector3(Target.position.x, Target.position.y + 0.5f);

            var m_firePointPosition = transform.position;

            DrawShootingLine(m_firePointPosition, whereToShoot, Color.red, 0.5f);

            yield return new WaitForSeconds(0.6f); //wait before shoot

            var hit2D = Physics2D.Raycast(m_firePointPosition,
                                                           whereToShoot - m_firePointPosition,
                                                           10f, 8);

            //PlayShootSound();

            DrawBulletTrailEffect(whereToShoot);
        }
    }

    private void DrawBulletTrailEffect(Vector3 whereToShoot)
    {
        Vector3 difference = whereToShoot - transform.position;
        difference.Normalize();

        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        var bullet = Instantiate(BulletTrailPrefab, transform.position,
                    Quaternion.Euler(0f, 0f, rotationZ));

        bullet.GetComponent<MoveBullet>().DamageAmount = DamageAmount;
    }

    private void DrawShootingLine(Vector3 startPoint, Vector3 endPoint, Color color, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = startPoint;
        myLine.AddComponent<LineRenderer>();

        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = TrailMaterial;

        lr.startColor = color;
        lr.endColor = color;

        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;

        lr.SetPosition(0, startPoint);
        lr.SetPosition(1, endPoint);

        Destroy(myLine, duration);
    }

    #endregion

    #region kamikaze

    private void InitializeChasing()
    {
        //start a new path to the target
        m_Seeker.StartPath(transform.position, Target.position, OnPathComplete);
        StopAllCoroutines();
        StartCoroutine(UpdatePath());
    }

    #region move in direction A*

    private IEnumerator PatrolingInRandomDirection()
    {
        var randX = Random.Range(m_MinPosition, m_MaxPosition);
        var randY = Random.Range(m_MinPosition, m_MaxPosition);

        var target = new Vector3(transform.position.x - randX, transform.position.y - randY);

        var waitBeforeMove = Random.Range(2f, 5f);

        yield return new WaitForSeconds(waitBeforeMove); //wait before moving

        m_Seeker.StartPath(transform.position, target, OnPathComplete);
    }

    private IEnumerator UpdatePath()
    {
        if (Target != null)
        {
            //start a new path to the target
            m_Seeker.StartPath(transform.position, Target.position, OnPathComplete);

            yield return new WaitForSeconds(1f / UpdateRate);

            StartCoroutine(UpdatePath());
        }
    }

    private void OnPathComplete(Path path)
    {
        if (!path.error)
        {
            m_Path = path;
            m_CurrentWaypoint = 0;

            if (!m_IsPlayerInAttackRange)
                StartCoroutine(PatrolingInRandomDirection());
        }
    }

    private void MoveInDirection()
    {
        if (m_Path != null & !m_IsDestroying)
        {
            if (m_CurrentWaypoint >= m_Path.vectorPath.Count)
            {
                if (m_PathIsEnded)
                    return;

                m_PathIsEnded = true;
            }
            else
            {
                m_PathIsEnded = false;

                var direction = (m_Path.vectorPath[m_CurrentWaypoint] - transform.position).normalized
                    * Speed * Time.fixedDeltaTime;

                m_Rigidbody.AddForce(direction, ForceMode2D.Force);

                if (Vector3.Distance(transform.position, m_Path.vectorPath[m_CurrentWaypoint]) < m_NextWaypointDistance)
                {
                    m_CurrentWaypoint++;
                }
            }
        }
    }

    #endregion

    #endregion
}
