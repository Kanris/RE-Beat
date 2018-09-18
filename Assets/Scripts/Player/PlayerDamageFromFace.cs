using UnityEngine;

public class PlayerDamageFromFace : MonoBehaviour {

    #region public fields

    public Player player;

    #endregion

    #region private methods

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            player.IsDamageFromFace = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            player.IsDamageFromFace = false;
        }
    }

    #endregion
}
