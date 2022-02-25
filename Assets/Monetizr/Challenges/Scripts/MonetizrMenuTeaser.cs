using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Challenges
{

    public class MonetizrMenuTeaser : PanelController
    {
        public Button button;
        public RawImage teaserImage;
        public float delayTime = 5f;
        public float moveTime = 1f;

        private int state = 0;
        private float progress = 0f;
        private Material m = null;
        private float delayTimeEnd = 0f;
        private float speed = 1f;
        private Rect uvRect = new Rect(0, 0.5f, 1.0f, 0.5f);

        void Update()
        {
            switch (state)
            {
                case 0:
                    progress += speed * Time.deltaTime / moveTime;

                    if (progress > 1.0f || progress < 0.0f)
                    {
                        progress = Mathf.Clamp(progress, 0f, 1f);
                        speed *= -1;
                        delayTimeEnd = Time.time + delayTime;
                        state = 1;
                    }
                    SetProgress(progress);

                    break;

                case 1:

                    if (Time.time > delayTimeEnd)
                    {
                        state = 0;
                    }

                    break;
            }

        }

        void SetProgress(float a)
        {
            uvRect.y = 0.5f * (1.0f - Tween(a));
            teaserImage.uvRect = uvRect;
        }

        float Tween(float k)
        {
            return 0.5f * (1f - Mathf.Cos(Mathf.PI * k));
        }

        internal override void PreparePanel(PanelId id, Action onComplete, List<MissionUIDescription> missionsDescriptions)
        {
            
        }

        internal override void FinalizePanel(PanelId id)
        {
            
        }
    }

}