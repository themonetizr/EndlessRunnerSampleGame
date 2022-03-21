using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Challenges
{

    public class NotifyPanel : PanelController
    {
        public Image banner;
        public Image rewardImage;
        public Text rewardAmount;
        public Text title;
        public Text text;
        public Image logo;
        public Button closeButton;
        public Text buttonText;

        //private Action onComplete;

        internal override void PreparePanel(PanelId id, Action onComplete, MissionUIDescription m)
        {
            this.onComplete = onComplete;
            this.panelId = id;

            switch(id) {
                case PanelId.CongratsNotification: PrepareCongratsPanel(m); break;
                case PanelId.StartNotification: PrepareNotificationPanel(m); break;
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
            }

            
        }

        //TODO: Fix notification panel!
        private void PrepareNotificationPanel(MissionUIDescription m)
        {
            if (MonetizrManager.Instance.HasChallengesAndActive())
            {
                var challengeId = MonetizrManager.Instance.GetActiveChallenge();

                banner.sprite = m.brandBanner;
                logo.sprite = m.brandLogo;

                string brandTitle = m.brandName;

                title.text = $"{brandTitle} video";
                text.text = $"<color=#F05627>Watch video</color> by {brandTitle} to get <color=#F05627>{m.reward} {m.rewardTitle}</color>";

                //buttonText.text = "Learn More";
                buttonText.text = "Got it!";

                rewardImage.gameObject.SetActive(false);
                rewardAmount.gameObject.SetActive(false);

                closeButton.onClick.AddListener(OnButtonPress);

                MonetizrManager.Analytics.TrackEvent("Notification shown");
                MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner);
            }
        }

        private void PrepareCongratsPanel(MissionUIDescription m)
        {
            if (MonetizrManager.Instance.HasChallengesAndActive())
            {
                var challengeId = MonetizrManager.Instance.GetActiveChallenge();

                banner.sprite = m.brandLogo;
                logo.sprite = m.brandLogo; 

                title.text = $"Congrats!";
                text.text = $"You got <color=#F05627>{m.reward} {m.rewardTitle}</color> from {m.brandName}";

                //buttonText.text = "Learn More";
                buttonText.text = "Awesome!";

                rewardImage.gameObject.SetActive(true);
                rewardAmount.gameObject.SetActive(true);

                closeButton.onClick.AddListener(OnButtonPress);

                MonetizrManager.Analytics.TrackEvent("Reward notification shown");
                MonetizrManager.Analytics.BeginShowAdAsset(AdType.RewardBanner);
            }
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