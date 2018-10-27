using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(RawImage))]
public class PlayVideo : MonoBehaviour {

    [SerializeField] private VideoPlayer m_VideoToPlay;

    private RawImage m_BackgroundImage;

	// Use this for initialization
	private void Start () {

        m_BackgroundImage = GetComponent<RawImage>();

        if (m_VideoToPlay != null)
            StartCoroutine(PlayBackground());
    }

    private IEnumerator PlayBackground()
    {
        m_VideoToPlay.Prepare();

        while (!m_VideoToPlay.isPrepared)
        {
            yield return new WaitForSeconds(0.05f);
        }

        m_BackgroundImage.texture = m_VideoToPlay.texture;
        m_VideoToPlay.Play();
    }

    public IEnumerator PlayBackgroundVideo(GameObject videoPlayer)
    {
        var videoToPlay = Instantiate(videoPlayer, transform).GetComponent<VideoPlayer>();

        videoToPlay.Prepare();

        while (!videoToPlay.isPrepared)
        {
            yield return new WaitForSeconds(0.05f);
        }

        m_BackgroundImage.texture = videoToPlay.texture;
        videoToPlay.Play();
        
        Destroy(videoToPlay.gameObject, 5f);
    }
}
