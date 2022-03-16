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
            Debug.Log("UpdateUI");

            CleanListView();

            if (MonetizrManager.Instance.HasChallengesAndActive())
            {
                hasSponsoredChallenges = true;
                AddSponsoredChallenges();
            }
                        
            AddUserdefineChallenges();
        }

        internal override void PreparePanel(PanelId id, Action onComplete)
        {
            hasSponsoredChallenges = false;

            //this.missionsDescriptions = missionsDescriptions;

            MonetizrManager.Analytics.TrackEvent("Reward center opened");

            MonetizrManager.HideTinyMenuTeaser();

            this.onComplete = onComplete;

            UpdateUI();
        }

        private void AddUserdefineChallenges()
        {
            foreach (var m in uiController.missionsDescriptions)
            {
                if (m.isSponsored)
                    continue;

                var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

                var item = go.GetComponent<MonetizrRewardedItem>();


                Debug.Log(m.missionTitle);

                item.UpdateWithDescription(this,m);
            }
        }

        private void AddSponsoredChallenges()
        {
            var challenges = MonetizrManager.Instance.GetAvailableChallenges();
            int curChallenge = 0;

            if (challenges.Count == 0)
                return;

            foreach (var m in uiController.missionsDescriptions)
            {
                if (!m.isSponsored)
                    continue;

                var ch = challenges[curChallenge];

                AddSponsoredChallenge(m,ch);

                curChallenge++;

                //if there's no room for sponsored campagn
                if (challenges.Count == curChallenge)
                    break;
            }
        }

        private void AddSponsoredChallenge(MissionUIDescription m, string ch)
        {
            string brandName = MonetizrManager.Instance.GetAsset<string>(ch, AssetsType.BrandTitleString);

            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.BrandBannerSprite);
            m.missionTitle = $"{brandName} video";
            m.missionDescription = $"Watch video by {brandName} and get 2 Energy Boosters";
            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.BrandRewardLogoSprite);
            m.progress = 1;

            //show video, then claim rewards if it's completed
            m.onClaimButtonPress = () => { OnVideoPlayPress(m); };

            var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

            var item = go.GetComponent<MonetizrRewardedItem>();


            Debug.Log(m.missionTitle);

            item.UpdateWithDescription(this, m);

            MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, ch);
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
            if (!missionDescription.isSponsored)
                MonetizrManager.CleanUserDefinedMissions();

            //play video or claim ready user-defined mission
            missionDescription.onClaimButtonPress.Invoke();

            if (!missionDescription.isSponsored)
                UpdateUI();
            
        }

        public void OnVideoPlayPress(MissionUIDescription m)
        {
            MonetizrManager.Analytics.TrackEvent("Claim button press");

            MonetizrManager._PlayVideo((bool isSkipped) => {

                if (!isSkipped)
                {   
                    if (MonetizrManager.Instance.HasChallengesAndActive())
                    {
                        var ch = MonetizrManager.Instance.GetActiveChallenge();

                        MonetizrManager.Instance.ClaimReward(ch);

                        m.onUserDefinedClaim.Invoke(m.reward);
                    }

                    MonetizrManager.ShowCongratsNotification(null);

                    
                }

            });
        }

        internal override void FinalizePanel(PanelId id)
        {
            if (!uiController.isVideoPlaying)
            {
                MonetizrManager.CleanUserDefinedMissions();
                MonetizrManager.ShowTinyMenuTeaser(null);
            }

            if (hasSponsoredChallenges)
            {
                MonetizrManager.Analytics.EndShowAdAsset(AdType.IntroBanner);
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