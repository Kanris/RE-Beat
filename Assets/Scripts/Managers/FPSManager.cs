using System.Collections;
using UnityEngine;
using TMPro;

public class FPSManager : MonoBehaviour {

    #region singleton

    public static FPSManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    #endregion

    [SerializeField] private TextMeshProUGUI m_FPSText;
    
    private string m_TextToDisplay;

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            var current = (int)(1f / Time.unscaledDeltaTime);
            m_TextToDisplay = "FPS: " + current;

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnGUI()
    {
        m_FPSText.text = m_TextToDisplay;
    }
}
