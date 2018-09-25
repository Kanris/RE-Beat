using UnityEngine;

public class ChangeFootStepsSound : MonoBehaviour {

    #region private fields

    [SerializeField] private Audio Sound; //foot sound

    #endregion

    #region private methods

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") )
        {
            if (collision.gameObject.GetComponent<FootSound>().Sound != Sound)
            {
                AudioManager.Instance.Stop(collision.gameObject.GetComponent<FootSound>().Sound);
                collision.gameObject.GetComponent<FootSound>().Sound = Sound;
            }
        }
    }

    #endregion
}
