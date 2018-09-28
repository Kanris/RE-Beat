using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SawTrigger : MonoBehaviour {

    #region private fields

    #region serialize fields
    
    [SerializeField] private Saw m_Saw; //saw script
    [SerializeField] private Audio ButtonSwitchAudio;

    #endregion

    private Animator m_Animator; //animator
    private bool m_IsSawMove; //if saw is moving

    #endregion

    #region private methods

    // Use this for initialization
    void Start () {

        m_Animator = GetComponent<Animator>(); //get animator

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsSawMove) //if player is on trigger and saw is not moving
        {
            StartCoroutine(MoveSaw()); //move saw

            PlayTriggerSound(); //play trigger sound
            ButtonAnimation("Pressed"); //play press button animation
        }
    }

    private int WhereToMove() //determine where to move
    {
        if (m_Saw.transform.position.x > transform.position.x)
        {
            return -1;
        }

        return 1;
    }

    private IEnumerator MoveSaw()
    {
        m_IsSawMove = true; //saw is moving

        yield return m_Saw.MoveWithHide(WhereToMove()); //move attached saw

        m_IsSawMove = false; //saw is not moving


        ButtonAnimation("Unpressed"); //play unpressed animation
    }

    private void ButtonAnimation(string animation)
    {
        m_Animator.SetTrigger(animation);
    }

    private void PlayTriggerSound()
    {
        AudioManager.Instance.Play(ButtonSwitchAudio);
    }

    #endregion
}
