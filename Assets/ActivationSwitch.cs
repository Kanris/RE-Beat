using UnityEngine;
using System.Collections;
using UnityStandardAssets._2D;

public class ActivationSwitch : MonoBehaviour
{
    [SerializeField] private DisappearPlatform m_PlatformToActivate;
    [SerializeField, Range(1, 5)] private float m_ShowTime = 2f;

    [Header("Additional")]
    [SerializeField] private InteractionUIButton m_InteractionUIButton;

    private bool m_IsQuitting; //is application closing
    private bool m_IsDestroying; //is switch destroying
    private Camera2DFollow m_Camera; //main camera on scene

    #region initialize
    // Start is called before the first frame update
    void Start()
    {
        m_InteractionUIButton.PressInteractionButton = ActivatePlatform; //set what is happening when player press button
        m_InteractionUIButton.SetActive(false); //hide item's ui

        m_Camera = Camera.main.GetComponent<Camera2DFollow>();

        //subscribe on is quitting method
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += SetIsQuitting; //if player is open start screen
        MoveToNextScene.IsMoveToNextScene += SetIsQuitting; //is player is move to the next sceen
    }

    #endregion

    private void ActivatePlatform()
    {
        //if switch is not destroying
        if (!m_IsDestroying)
        {
            m_IsDestroying = true; //indicate that switch is destroying

            StartCoroutine(ActivatePlatformWithShow()); //show camera change
        }
    }

    private IEnumerator ActivatePlatformWithShow()
    {
        if (m_PlatformToActivate != null)
        {
            Time.timeScale = 0f;

            StartCoroutine(m_Camera.SetTarget(m_PlatformToActivate.gameObject.transform, 1f)); //show change state

            m_PlatformToActivate.SetActivateByCompanion(false);
            GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<PlatformerCharacter2D>().enabled = false; //do not allow player to move

            yield return new WaitForSecondsRealtime(m_ShowTime); //time before return camera on player

            GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<PlatformerCharacter2D>().enabled = true; //return player's control
            yield return m_Camera.SetTarget(GameMaster.Instance.m_Player.transform.GetChild(0).transform, 1f); //delay to apply damping

            GameMaster.Instance.SaveState(transform.name, 0, GameMaster.RecreateType.Object); //save gameObject state
            Time.timeScale = 1f;
        }
        else
        {
            Debug.LogError($"ActivationSwitch.ActivatePlaformWithShow: {transform.name} hadn't any attached platform; Destroying switch without show");
        }

        Destroy(gameObject); //destroy this gameObject
    }

    #region triggers
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetInteractionUIButton(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetInteractionUIButton(false);
        }
    }
    #endregion

    private void SetInteractionUIButton(bool value)
    {
        m_InteractionUIButton.SetIsPlayerNear(value);
        m_InteractionUIButton.SetActive(value);
    }

    #region is quitting
    //change quitting value
    private void SetIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting && !m_IsDestroying) //is application is not closing
        {
            m_PlatformToActivate.SetActivateByCompanion(false);
        }
    }
    #endregion
}
