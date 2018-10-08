using UnityEngine;

public class ObjectFollow : MonoBehaviour {

    [SerializeField] private Transform m_FollowTransform;

    private Vector3 m_Offset;

    private void Start()
    {
        if (m_FollowTransform == null)
            Destroy(this);

        m_Offset = transform.localPosition;
    }

    // Update is called once per frame
    void Update () {

        transform.position = m_FollowTransform.position.Add(m_Offset);

	}
}
