using UnityEngine;

//class that handle input from users (for interaction ui)
public class InteractionUIButton : MonoBehaviour
{
    public delegate void VoidDelegate();
    public VoidDelegate PressInteractionButton; //invoke method when player pressed need button

    public enum InteractionType { ArrowUp, PickUp } //interaction types
    [Header("Interaction Type")]
    [SerializeField] private InteractionType m_InteractionType; //current ui interaction button

    [Header("Additional")]
    [SerializeField] private GameObject m_InteractionUI; //parent canvas

    public bool m_IsPlayerNear { get; private set; }

    // Update is called once per frame
    private void Update()
    {
        if (m_IsPlayerNear && InputControlManager.Instance.IsCanUseSubmitButton())
        {
            //if current ui interaction button is ArrowUp
            if (m_InteractionType == InteractionType.ArrowUp)
            {
                //if ArrowUp button pressed
                if (InputControlManager.Instance.IsUpperButtonsPressed())
                {
                    //call attached method
                    PressInteractionButton?.Invoke();
                }
            }
            //if current ui interaction button is PickUp
            else if (m_InteractionType == InteractionType.PickUp)
            {
                //if PickUp button pressed
                if (InputControlManager.Instance.IsPickupPressed())
                {
                    //call attached method
                    PressInteractionButton?.Invoke();
                }
            }
        }
    }

    public void SetIsPlayerNear(bool value)
    {
        m_IsPlayerNear = value;
    }

    public void SetActive(bool value)
    {
        m_InteractionUI.SetActive(value);
    }

    public bool ActiveSelf()
    {
        return m_InteractionUI.activeSelf;
    }
}
