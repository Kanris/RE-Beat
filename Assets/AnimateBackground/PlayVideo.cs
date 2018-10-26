using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideo : MonoBehaviour {

    [SerializeField] private VideoPlayer m_VideoToPlay;

	// Use this for initialization
	void Start () {

        StartCoroutine(PlayBackground());
    }

    private IEnumerator PlayBackground()
    {
        m_VideoToPlay.Prepare();

        while (!m_VideoToPlay.isPrepared)
        {
            yield return new WaitForSeconds(0.05f);
        }

        GetComponent<RawImage>().texture = m_VideoToPlay.texture;
        m_VideoToPlay.Play();
    }
}
