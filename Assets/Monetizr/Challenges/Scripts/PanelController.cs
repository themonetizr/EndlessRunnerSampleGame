using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Monetizr.Challenges
{
    public abstract class PanelController : MonoBehaviour
    {
        private Animator animator;
        private CanvasGroup canvasGroup;

        public abstract void PreparePanel();

        protected void Awake()
        {
            animator = GetComponent<Animator>();

            if (canvasGroup == null)
                canvasGroup = gameObject.GetComponent<CanvasGroup>();

            Assert.IsNotNull(animator);
            Assert.IsNotNull(canvasGroup);
        }

        internal void EnableInput(bool enable)
        {
            canvasGroup.blocksRaycasts = enable;
        }

        internal void SetActive(bool active, bool immediately = false)
        {
            if (active)
            {
                EnableInput(true);

                gameObject.SetActive(true);
                animator.Play("PanelAnimator_Show");
            }
            else
            {
                EnableInput(false);

                if (!immediately)
                {
                    animator.Play("PanelAnimator_Hide");
                }
                else
                    OnAnimationHide();

            }
        }

        internal GameObject GetActiveElement(string name)
        {
            return transform.Find(name).gameObject;
        }

        internal GameObject GetElement(string name)
        {
            foreach (var childTransform in transform.GetComponentsInChildren<Transform>(true))
            {
                if (childTransform.name == name)
                    return childTransform.gameObject;
            }

            return null;
        }

        internal void OnAnimationShow()
        {

        }

        private void OnAnimationHide()
        {
            gameObject.SetActive(false);
        }
    }

}