using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestItem : MonoBehaviour {

    [SerializeField] Item item;

    public void AddToTheInventory()
    {
        PlayerStats.PlayerInventory.Add(item, GetComponent<Image>().sprite.name);
        AnnouncerManager.Instance.DisplayAnnouncerMessage(GetAnnouncerMessage());

        if (GameMaster.Instance != null)
            GameMaster.Instance.SaveState(transform.parent.parent.parent.name, gameObject.name, GameMaster.RecreateType.ChestItem);

        Destroy(gameObject);
    }

    private AnnouncerManager.Message GetAnnouncerMessage()
    {
        return new AnnouncerManager.Message(item.Name + " add to the inventory");
    }
}
