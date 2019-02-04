using UnityEngine;

public class ReturnPoint : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if player is in return's point trigger
        if (collision.CompareTag("Player"))
        {
            //rewrite current return point
            GameMaster.Instance.SetReturnPoint(gameObject.transform);
        }
    }

}
