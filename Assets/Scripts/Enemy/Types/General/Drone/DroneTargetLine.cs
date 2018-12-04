using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DroneTargetLine : MonoBehaviour {

    [SerializeField] private Transform m_BasePoint;
    [SerializeField] private LineRenderer m_TargetLine;

    // Update is called once per frame
    void Update () {

        m_TargetLine.SetPosition(0, m_BasePoint.position);

    }

    public void SetTarget(Vector3 targetPosition)
    {
        m_TargetLine.SetPosition(1, targetPosition);
        m_TargetLine.gameObject.SetActive(true);
    }
}
