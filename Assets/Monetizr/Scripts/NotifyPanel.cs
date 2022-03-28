using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class NotifyPanel : PanelController
    {
        public Image banner;
        public Image rewardImageBackgroud;
        public Image rewardImage;
        public Text rewardAmount;
        public Text title;
        public Text text;
        public Image logo;
        public Button closeButton;
        public Text buttonText;

        //private Action onComplete;

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, MissionUIDescription m)
        {
            this.onComplete = onComplete;
            this.panelId = id;

            closeButton.onClick.AddListener(OnButtonPress);

            switch (id)
            {
                case PanelId.CongratsNotification: PrepareCongratsPanel(m); break;
                case PanelId.StartNotification: PrepareNotificationPanel(m); break;
                case PanelId.SurveyNotification: PrepareSurveyNotificationPanel(m); break;
            }
        }

        internal override void FinalizePanel(PanelId id)
        {
            switch (id)
            {
                case PanelId.CongratsNotification:
                    MonetizrManager.Analytics.EndShowAdAsset(AdType.RewardBanner);
                    break;

                case PanelId.StartNotification:
                    MonetizrManager.Analytics.EndShowAdAsset(AdType.IntroBanner);
                    break;

                case PanelId.SurveyNotification:
                    MonetizrManager.Analytics.EndShowAdAsset(AdType.RewardBanner);
                    break;
            }


        }



        private void PrepareNotificationPanel(MissionUIDescription m)
        {
            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = m.brandBanner;
            logo.sprite = m.brandLogo;

            string brandTitle = m.brandName;

            title.text = $"{brandTitle} video";
            text.text = $"<color=#F05627>Watch video</color> by {brandTitle} to get <color=#F05627>{m.reward} {m.rewardTitle}</color>";

            //buttonText.text = "Learn More";
            buttonText.text = "Got it!";

            rewardImage.gameObject.SetActive(false);
            rewardAmount.gameObject.SetActive(false);
            rewardImageBackgroud.gameObject.SetActive(false);




            MonetizrManager.Analytics.TrackEvent("Notification shown", m);
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner);

        }

        private void PrepareCongratsPanel(MissionUIDescription m)
        {

            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = m.brandRewardBanner;
            logo.sprite = m.brandLogo;

            title.text = $"Congrats!";
            text.text = $"You got <color=#F05627>{m.reward} {m.rewardTitle}</color> from {m.brandName}";

            //buttonText.text = "Learn More";
            buttonText.text = "Awesome!";

            rewardImage.gameObject.SetActive(true);
            rewardImageBackgroud.gameObject.SetActive(true);
            rewardAmount.gameObject.SetActive(true);

            rewardImage.sprite = m.rewardIcon;


            MonetizrManager.Analytics.TrackEvent("Reward notification shown", m);
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.RewardBanner);

        }

        private void PrepareSurveyNotificationPanel(MissionUIDescription m)
        {

            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = m.brandRewardBanner;
            logo.sprite = m.brandLogo;

            title.text = $"Survey!";
            //text.text = $"Please spend some time and  <color=#F05627>{m.reward} {m.rewardTitle}</color> from {m.brandName}";

            text.text = $"<color=#F05627>Complete the survey</color>\nby {m.brandName} to get\n<color=#F05627>{m.reward} {m.rewardTitle}</color>";

            //buttonText.text = "Learn More";
            buttonText.text = "Awesome!";

            rewardImage.gameObject.SetActive(true);
            rewardImageBackgroud.gameObject.SetActive(true);
            rewardAmount.gameObject.SetActive(true);

            rewardImage.sprite = m.rewardIcon;



            MonetizrManager.Analytics.TrackEvent("Survey notification shown", m);
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.RewardBanner);

        }

        private new void Awake()
        {
            base.Awake();


        }

        public void OnButtonPress()
        {
            SetActive(false);
        }

    }

}