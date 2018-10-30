using UnityEngine;
using Cinemachine;
using System.Collections;

public class CinemachineFollow : MonoBehaviour {

    [SerializeField] private CinemachineVirtualCamera m_VirtualCamera;

    public void Start()
    {
#if MOBILE_INPUT
        m_VirtualCamera.m_Lens.OrthographicSize = 5f;
#endif
    }

    public void SetCameraTarget(Transform target)
    {
        m_VirtualCamera.Follow = target;
        m_VirtualCamera.m_Lens.OrthographicSize = 5f;
    }

    public void PlayHitEffect()
    {
        StartCoroutine(PlayCameraHitAnimation());
    }

    public void PlayLowHealthEffect()
    {
        Camera.main.GetComponent<Kino.DigitalGlitch>().enabled = true;
    }

    public void StopLowHealthEffect()
    {
        Camera.main.GetComponent<Kino.DigitalGlitch>().enabled = false;
    }

    private IEnumerator PlayCameraHitAnimation()
    {

        Camera.main.GetComponent<Kino.AnalogGlitch>().enabled = true;

        yield return new WaitForSeconds(0.5f);

        Camera.main.GetComponent<Kino.AnalogGlitch>().enabled = false;
    }
}
