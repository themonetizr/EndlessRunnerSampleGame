﻿using System;
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
            this.onComplete = onComplete;

            foreach (var ch in MonetizrManager.Instance.GetAvailableChallenges())
            {
                string brandName = MonetizrManager.Instance.GetAsset<string>(ch, AssetsType.BrandTitleString);

                MissionUIDescription m = new MissionUIDescription()
                {
                    brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.BrandRewardBannerSprite),
                    missionTitle = $"{brandName} video",
                    missionDescription = $"Watch video by {brandName} and get 2 Energy Boosters",
                    missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.BrandRewardLogoSprite),

                    rewardIcon = null,
                    reward = 2,
                    progress = 1,
                    onClaimButtonPress = () => { OnVideoPlayPress(); },
                    isSponsored = true,
                };
                                          
                var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

                var item = go.GetComponent<MonetizrRewardedItem>();


                Debug.Log(m.missionTitle);

                item.UpdateWithDescription(m);
            }

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

        internal override void FinalizePanel(PanelId id)
        {
            foreach(var c in contentRoot.GetComponentsInChildren<Transform>())
            {
                if(c != contentRoot)
                    Destroy(c.gameObject);
            }
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