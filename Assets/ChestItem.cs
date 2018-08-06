using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestItem : MonoBehaviour {

    [SerializeField] Item item;

    public void AddToTheInventory()
    {
        PlayerStats.PlayerInventory.Add(item);
        AnnouncerManager.Instance.DisplayAnnouncerMessage(GetAnnouncerMessage());

        if (GameMaster.Instance != null)
            GameMaster.Instance.SaveChestState(gameObject.name);

        Destroy(gameObject);
    }

    private AnnouncerManager.Message GetAnnouncerMessage()
    {
        return new AnnouncerManager.Message(item.Name + " add to the inventory");
    }
}
