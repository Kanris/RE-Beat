using UnityEngine;

[RequireComponent(typeof(DistanceJoint2D), typeof(LineRenderer))]
public class DrawLine : MonoBehaviour {

    [SerializeField] private DistanceJoint2D Joint;
    [SerializeField] private LineRenderer Line;

    private void Start()
    {
        var startPoint = Joint.connectedAnchor;
        Line.SetPosition(0, startPoint);

        /*capsule = gameObject.AddComponent<CapsuleCollider2D>();
        capsule.isTrigger = true;

        capsule.offset = new Vector2(0, 1);
        capsule.size = new Vector2(0.2f, 2.8f);*/
    }

    // Update is called once per frame
    void FixedUpdate () {
    
        Line.SetPosition(1, transform.position);

        /*capsule.transform.position = Joint.connectedAnchor + 
            ( new Vector2( transform.position.x, transform.position.y ) - Joint.connectedAnchor) / 2;*/
    }
}
