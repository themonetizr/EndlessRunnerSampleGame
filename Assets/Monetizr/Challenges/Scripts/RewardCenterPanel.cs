using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.Challenges
{

    public class RewardCenterPanel : PanelController
    {
        public Transform contentRoot;
        public MonetizrRewardedItem itemUI;
    
        private new void Awake()
        {
            base.Awake();


        }

        internal override void PreparePanel(PanelId id, Action onComplete, List<MissionUIDescription> missionsDescriptions)
        {
            Debug.Log("PreparePanel");
            foreach(var m in missionsDescriptions)
            {
                var go = GameObject.Instantiate<GameObject>(itemUI.gameObject,contentRoot);

                var item = go.GetComponent<MonetizrRewardedItem>();


                Debug.Log(m.missionTitle);

                item.UpdateWithDescription(m);
            }
        }

        public void OnButtonPress()
        {
            SetActive(false);
        }

        public void OnVideoPlayPress()
        {
            MonetizrManager._PlayVideo((bool isSkipped) => {

                if(!isSkipped)
                    MonetizrManager.ShowCongratsNotification(null);
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