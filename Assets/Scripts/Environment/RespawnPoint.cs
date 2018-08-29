using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour {

    [SerializeField] private bool isLight;

    private Transform m_Flame;

    private void Start()
    {
        InitializeFlame();
    }

    private void InitializeFlame()
    {
        m_Flame = gameObject.transform.GetChild(0);

        if (m_Flame == null)
        {
            Debug.LogError("RespawnPoint: Can't find flame in child");
        }

        SetActiveFlame(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_Flame.gameObject.activeSelf)
        {
            GameMaster.Instance.ChangeRespawnPoint(gameObject.transform);

            SetActiveFlame(true);
            ChangePlayerMaterial(collision);

            SaveLoadManager.Instance.SaveGameData();
        }
    }

    public void SetActiveFlame(bool value)
    {
        m_Flame.gameObject.SetActive(value);
    }

    private void ChangePlayerMaterial(Collider2D collision)
    {
        collision.GetComponent<MaterialChange>().Change(isLight);
    }
}
