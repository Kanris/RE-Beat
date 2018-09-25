using UnityEngine;

public class PlaySoundOnTrigger : MonoBehaviour {

    #region private fields

    [SerializeField] private Audio Sound;

    #endregion

    #region private methods

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SoundManager(collision.tag);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        SoundManager(collision.tag, false);
    }

    private void SoundManager(string name, bool PlaySound = true)
    {
        if (!string.IsNullOrEmpty(Sound)) //if sound attached
        {
            if (name == "Player") //if collision is a player
            {
                if (PlaySound)
                {
                    AudioManager.Instance.Play(Sound, true);
                }
                else
                {
                    AudioManager.Instance.Stop(Sound, true);
                }
            }
        }
        else
        {
            Debug.LogError("PlaySoundOnTrigger.SoundManager: Sound name can't be empty");
        }

    }

    #endregion
}
