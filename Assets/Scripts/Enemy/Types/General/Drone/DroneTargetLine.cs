using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DroneTargetLine : MonoBehaviour {

    [SerializeField] private Transform m_BasePoint; //transform that is start for line
    [SerializeField] private LineRenderer m_TargetLine; //line to change

    // Update is called once per frame
    void Update () {

        m_TargetLine.SetPosition(0, m_BasePoint.position); //start of line has to stay on base transform

    }

    public void SetTarget(Vector3 targetPosition)
    {
        m_TargetLine.SetPosition(0, m_BasePoint.position); //place start point on base
        m_TargetLine.SetPosition(1, targetPosition); //

        m_TargetLine.gameObject.SetActive(true);
    }
}
