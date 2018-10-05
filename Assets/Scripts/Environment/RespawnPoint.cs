using UnityEngine;

public class RespawnPoint : MonoBehaviour {

    #region private fields

    [Header("Light")]
    [SerializeField] private bool isLight;

    [Header("Effects")]
    [SerializeField] private Audio audioSave;

    private Transform m_Flame;

    #endregion

    #region private methods

    private void Start()
    {
        m_Flame = gameObject.transform.GetChild(0); //initialize flame animation
        m_Flame.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_Flame.gameObject.activeSelf) //if player is in respawn point trigger and respawn isn't active
        {
            GameMaster.Instance.ChangeRespawnPoint(gameObject.transform); //change player respawn point

            SetActiveFlame(true); //enable flame animation
            ChangePlayerMaterial(collision); //if respawn point has light on it change player material

            SaveLoadManager.Instance.SaveGameData(audioSave); //save game data
        }
    }

    private void ChangePlayerMaterial(Collider2D collision)
    {
        collision.GetComponent<MaterialChange>().Change(isLight); //if respawn point has light on it change player material
    }

    #endregion

    #region public methods

    public void SetActiveFlame(bool value)
    {
        m_Flame.gameObject.SetActive(value);
    }

    #endregion
}
