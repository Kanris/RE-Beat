using UnityEngine;
using Cinemachine;

public class CinemachineFollow : MonoBehaviour {

    [SerializeField] private CinemachineVirtualCamera m_VirtualCamera;

    public void Start()
    {
#if MOBILE_INPUT
        m_VirtualCamera.m_Lens.OrthographicSize = 4f;
#endif
    }

    public void SetCameraTarget(Transform target)
    {
        m_VirtualCamera.Follow = target;
    }

}
