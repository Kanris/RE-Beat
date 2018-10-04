using System;
using UnityEngine;

namespace UnityStandardAssets.CrossPlatformInput
{
    public class ButtonHandler : MonoBehaviour
    {
        private Animator m_Animator;

        public string Name;

        private void Start()
        {
            m_Animator = GetComponent<Animator>();
        }

        void OnEnable()
        {

        }

        public void SetDownState()
        {
            if (m_Animator != null) m_Animator.SetTrigger("Pressed");

            CrossPlatformInputManager.SetButtonDown(Name);
        }


        public void SetUpState()
        {
            if (m_Animator != null) m_Animator.SetTrigger("Unpressed");

            CrossPlatformInputManager.SetButtonUp(Name);
        }


        public void SetAxisPositiveState()
        {
            CrossPlatformInputManager.SetAxisPositive(Name);
        }


        public void SetAxisNeutralState()
        {
            CrossPlatformInputManager.SetAxisZero(Name);
        }


        public void SetAxisNegativeState()
        {
            CrossPlatformInputManager.SetAxisNegative(Name);
        }

        public void Update()
        {

        }
    }
}
