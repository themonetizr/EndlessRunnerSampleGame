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
        private bool hasSponsoredChallenges;
    
        private new void Awake()
        {
            base.Awake();
            
        }

        internal override void PreparePanel(PanelId id, Action onComplete, List<MissionUIDescription> missionsDescriptions)
        {
            hasSponsoredChallenges = false;

            MonetizrManager.Analytics.TrackEvent("Reward center opened");

            MonetizrManager.HideTinyMenuTeaser();

            this.onComplete = onComplete;

            CleanListView();

            if (MonetizrManager.Instance.HasChallenges())
            {
                hasSponsoredChallenges = true;
                AddSponsoredChallenges();
            }
                        
            AddUserdefineChallenges(missionsDescriptions);
        }

        private void AddUserdefineChallenges(List<MissionUIDescription> missionsDescriptions)
        {
            foreach (var m in missionsDescriptions)
            {
                var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

                var item = go.GetComponent<MonetizrRewardedItem>();


                Debug.Log(m.missionTitle);

                item.UpdateWithDescription(m);
            }
        }

        private void AddSponsoredChallenges()
        {
            foreach (var ch in MonetizrManager.Instance.GetAvailableChallenges())
            {
                string brandName = MonetizrManager.Instance.GetAsset<string>(ch, AssetsType.BrandTitleString);

                MissionUIDescription m = new MissionUIDescription()
                {
                    brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.BrandBannerSprite),
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

                MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner);

                //only one challenge per time
                break;
            }
        }

        private void CleanListView()
        {
            foreach (var c in contentRoot.GetComponentsInChildren<Transform>())
            {
                if (c != contentRoot)
                    Destroy(c.gameObject);
            }
        }

        public void OnButtonPress()
        {
            SetActive(false);
        }

        public void OnVideoPlayPress()
        {
            MonetizrManager.Analytics.TrackEvent("Claim button press");

            MonetizrManager._PlayVideo((bool isSkipped) => {

                if (!isSkipped)
                {   
                    if (MonetizrManager.Instance.HasChallenges())
                    {
                        var ch = MonetizrManager.Instance.GetAvailableChallenges()[0];

                        MonetizrManager.Instance.ClaimReward(ch);
                    }

                    MonetizrManager.ShowCongratsNotification(null);
                }
            });
        }

        internal override void FinalizePanel(PanelId id)
        {
            if (hasSponsoredChallenges)
            {
                MonetizrManager.Analytics.EndShowAdAsset(AdType.IntroBanner);
            }

            MonetizrManager.ShowTinyMenuTeaser(null);
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