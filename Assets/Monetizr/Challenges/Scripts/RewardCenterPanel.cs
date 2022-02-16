using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.Challenges
{

    public class RewardCenterPanel : PanelController
    {
        private new void Awake()
        {
            base.Awake();


        }

        public override void PreparePanel(Action onComplete)
        {

        }

        public void OnButtonPress()
        {
            SetActive(false);
        }

        public void OnVideoPlayPress()
        {
            MonetizrManager.PlayVideo((bool isSkipped) => {

                if(!isSkipped)
                    MonetizrManager.ShowStartupNotification(null);
            });
        }

        // Start is called before the first frame update
        //void Start()
        //{

        //}

        //// Update is called once per frame
        //void Update()
        //{

        //}
    }

}