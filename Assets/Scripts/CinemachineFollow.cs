using UnityEngine;
using Cinemachine;
using System.Collections;

public class CinemachineFollow : MonoBehaviour {

    [SerializeField] private CinemachineVirtualCamera m_VirtualCamera;

    [Header("Shake")]
    private float m_ShakeAmount = 1f;
    private float m_Duration = 1f;

    private bool m_IsShake = false;
    private CinemachineBasicMultiChannelPerlin m_VirtualCameraNoise;

    public void Start()
    {
#if MOBILE_INPUT
        m_VirtualCamera.m_Lens.OrthographicSize = 5f;
#endif

        m_VirtualCameraNoise = m_VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void SetCameraTarget(Transform target)
    {
        m_VirtualCamera.Follow = target;
    }

    public void ShakeCam()
    {
        if (!m_IsShake)
        {
            m_IsShake = true;
            StartCoroutine(StartShake());
        }
    }

    [ContextMenu("Emulate hit")]
    public void EmulateHit()
    {
        ShakeCam();
    }

    private IEnumerator StartShake()
    {
        m_VirtualCameraNoise.m_AmplitudeGain = m_ShakeAmount;

        yield return new WaitForSeconds(m_Duration);

        m_VirtualCameraNoise.m_AmplitudeGain = 0f;

        m_IsShake = false;


    }

}
