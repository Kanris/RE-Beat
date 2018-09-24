using UnityEngine;
using Cinemachine;

public class CinemachineFollow : MonoBehaviour {

    [SerializeField] private CinemachineVirtualCamera m_VirtualCamera;

    public void SetCameraTarget(Transform target)
    {
        m_VirtualCamera.Follow = target;
    }

}
