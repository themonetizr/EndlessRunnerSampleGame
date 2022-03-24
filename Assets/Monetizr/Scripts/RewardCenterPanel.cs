using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Challenges
{

    public class RewardCenterPanel : PanelController
    {
        public Transform contentRoot;
        public MonetizrRewardedItem itemUI;
        private bool hasSponsoredChallenges;
        public Text headerText;
        public Image background;

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

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, MissionUIDescription m)
        {
            hasSponsoredChallenges = false;

            //this.missionsDescriptions = missionsDescriptions;



            MonetizrManager.Analytics.TrackEvent("Reward center opened",null);

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

                item.UpdateWithDescription(this, m);
            }
        }

        private void AddSponsoredChallenges()
        {
            var challenges = MonetizrManager.Instance.GetAvailableChallenges();
            var activeChallenge = MonetizrManager.Instance.GetActiveChallenge();
            int curChallenge = 0;

            if (challenges.Count == 0)
                return;

            //put active challenge to the first place
            challenges.Remove(activeChallenge);
            challenges.Insert(0, activeChallenge);

            foreach (var m in uiController.missionsDescriptions)
            {
                if (!m.isSponsored)
                    continue;

                var ch = challenges[curChallenge];

                if (ch == activeChallenge)
                {
                    var color = MonetizrManager.Instance.GetAsset<Color>(ch, AssetsType.HeaderTextColor);

                    if (color != default(Color))
                        headerText.color = color;


                    var bgSprite = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.TiledBackgroundSprite);

                    if (bgSprite != default(Sprite))
                        background.sprite = bgSprite;
                }



                AddSponsoredChallenge(m, ch);

                curChallenge++;

                //if there's no room for sponsored campagn
                if (challenges.Count == curChallenge)
                    break;
            }
        }

        private void AddSponsoredChallenge(MissionUIDescription m, string campaignId)
        {
            string brandName = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.BrandTitleString);

            m.campaignId = campaignId;
            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.missionTitle = $"{brandName} video";
            m.missionDescription = $"Watch video by {brandName} and get {m.reward} {m.rewardTitle}";
            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardLogoSprite);
            m.progress = 1;
            m.brandName = brandName;
            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandLogoSprite); ;
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardBannerSprite);


            //show video, then claim rewards if it's completed
            m.onClaimButtonPress = () => { OnVideoPlayPress(campaignId, m); };

            var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

            var item = go.GetComponent<MonetizrRewardedItem>();


            Debug.Log(m.missionTitle);

            item.UpdateWithDescription(this, m);

            MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, campaignId);
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

        public void OnClaimRewardComplete(MissionUIDescription m, bool isSkipped)
        {
            MonetizrManager.ShowRewardCenter();

            //if (!isSkipped)
            {
                m.onUserDefinedClaim.Invoke(m.reward);

                MonetizrManager.ShowCongratsNotification(null, m);

                //MonetizrManager.Instance.ClaimReward(campaignId);

                //TODO: request tasks again!        
            }
        }

        public void OnVideoPlayPress(string campaignId, MissionUIDescription m)
        {
            MonetizrManager.Analytics.TrackEvent("Claim button press",m);

            var htmlPath = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.Html5PathString);

            if (htmlPath != null)
            {
                MonetizrManager.ShowHTML5((bool isSkipped) => { OnClaimRewardComplete(m, false); }, m);
            }
            else
            {
                var videoPath = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.VideoFilePathString);

                //MonetizrManager._PlayVideo(videoPath, (bool isSkipped) => { OnClaimRewardComplete(m, isSkipped); });

                MonetizrManager.ShowWebVideo((bool isSkipped) => { OnClaimRewardComplete(m, isSkipped); }, m);
            }
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