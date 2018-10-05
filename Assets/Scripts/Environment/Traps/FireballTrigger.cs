using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FireballTrigger : MonoBehaviour {


    #region private fields

    [Header("Fireball spawn location")]
    [SerializeField] private Transform m_ThrowerTransform; //from where throw fireballs

    #region enum

    public enum Direction { left, right } //direction where to move
    [Header("Fireball properties")]
    public Direction FireballDirection; //current fireball direction

    #endregion
    [SerializeField, Range(1, 10)] private int Count = 3; //fireballs count
    [SerializeField] GameObject m_FireballGameObject;


    [Header("Effects")]
    [SerializeField] private Audio ButtonSwitchAudio;

    private Animator m_Animator; //trigger animation
    private bool isCreatingFireballs; //is player triggered trap

    #endregion

    #region private methods

    private void Start()
    {
        m_Animator = GetComponentInChildren<Animator>(); //get reference to trigger animation
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !isCreatingFireballs) //if player on button and fireballs isn't creating
        {
            ButtonAnimation("Pressed"); //play pressed animation
            PlayTriggerSound(); //play trigger sound
            StartCoroutine(CreateFireballs()); //create fireballs
        }
    }

    #region fireball

    private IEnumerator CreateFireballs()
    {
        if (m_FireballGameObject != null) //if there is reference to the fireball
        {
            isCreatingFireballs = true; //notify that we creating fireballs
            var fireballDirection = GetFireballDirection(); //get fireballs move directions

            for (int index = 0; index < Count; index++) //creates need amount of fireballs
            {
                var fireball = Instantiate(m_FireballGameObject, m_ThrowerTransform.position + fireballDirection, m_ThrowerTransform.rotation); //instantiate fireballs
                fireball.GetComponent<Fireball>().Direction = fireballDirection; //set fireball direction

                yield return new WaitForSeconds(1f); //wait before create next fireball
            }
        }

        isCreatingFireballs = false; //notify that all fireballs created
        ButtonAnimation("Unpressed"); //play unpressed animation
    }

    private Vector3 GetFireballDirection()
    {
        var fireballDirection = Vector3.right; //by defaul fireball move to the right

        if (FireballDirection == Direction.left) //if direction is left
        {
            fireballDirection = Vector3.left; //change fireball direction to the left
        }

        return fireballDirection;
    }

    #endregion

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
