using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAppearOnTrigger : MonoBehaviour {

    [SerializeField] private GameObject ObjectToAppear;
    [SerializeField] private bool DestroyOnTrigger;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AppearObject();
            DestroyThisTrigger();
        }
    }

    private void AppearObject()
    {
        if (ObjectToAppear != null)
        {
            ObjectToAppear.SetActive(true);
        }
        else
        {
            Debug.LogError("ObjectAppearOnTrigger.AppearObject: ObjectToAppear is not assigned.");
        }
    }

    private void DestroyThisTrigger()
    {
        if (DestroyOnTrigger)
        {
            GameMaster.Instance.SaveState(gameObject.name, 0, GameMaster.RecreateType.Object);
            Destroy(gameObject);
        }
    }
}
