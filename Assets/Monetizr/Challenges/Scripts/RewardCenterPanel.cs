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
        //public List<MissionUIDescription> missionsDescriptions;
        
        private new void Awake()
        {
            base.Awake();
            
        }
        
        internal void UpdateUI()
        {
            CleanListView();

            if (MonetizrManager.Instance.HasChallengesAndActive())
            {
                hasSponsoredChallenges = true;
                AddSponsoredChallenges();
            }
                        
            AddUserdefineChallenges(uiController.missionsDescriptions);
        }

        internal override void PreparePanel(PanelId id, Action onComplete, List<MissionUIDescription> missionsDescriptions)
        {
            hasSponsoredChallenges = false;

            //this.missionsDescriptions = missionsDescriptions;

            MonetizrManager.Analytics.TrackEvent("Reward center opened");

            MonetizrManager.HideTinyMenuTeaser();

            this.onComplete = onComplete;

            UpdateUI();
        }

        private void AddUserdefineChallenges(List<MissionUIDescription> missionsDescriptions)
        {
            foreach (var m in missionsDescriptions)
            {
                var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

                var item = go.GetComponent<MonetizrRewardedItem>();


                Debug.Log(m.missionTitle);

                item.UpdateWithDescription(this,m);
            }
        }

        private void AddSponsoredChallenges()
        {
            var ch = MonetizrManager.Instance.GetActiveChallenge();
            
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

            item.UpdateWithDescription(this,m);

            MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner);

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

        internal void ButtonPressed(ButtonController buttonController, MissionUIDescription missionDescription)
        {
            MonetizrManager.CleanUserDefinedMissions();

            missionDescription.onClaimButtonPress.Invoke();

            UpdateUI();
        }


        public void OnVideoPlayPress()
        {
            MonetizrManager.Analytics.TrackEvent("Claim button press");

            MonetizrManager._PlayVideo((bool isSkipped) => {

                if (!isSkipped)
                {   
                    if (MonetizrManager.Instance.HasChallengesAndActive())
                    {
                        var ch = MonetizrManager.Instance.GetActiveChallenge();

                        //MonetizrManager.Instance.ClaimReward(ch);
                    }

                    MonetizrManager.ShowCongratsNotification(null);
                }
            });
        }

        internal override void FinalizePanel(PanelId id)
        {
            MonetizrManager.CleanUserDefinedMissions();

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