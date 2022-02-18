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

        internal override void PreparePanel(PanelId id, Action onComplete, List<MissionUIDescription> missionsDescriptions)
        {
            this.onComplete = onComplete;
            this.panelId = id;

            switch(id) {
                case PanelId.CongratsNotification: PrepareCongratsPanel(); break;
                case PanelId.StartNotification: PrepareNotificationPanel(); break;
            }
        }

        private void PrepareNotificationPanel()
        {
            var challenges = MonetizrManager.Instance.GetAvailableChallenges();

            if (challenges.Count > 0)
            {
                var challengeId = challenges[0];

                banner.sprite = MonetizrManager.Instance.GetAsset<Sprite>(challengeId, AssetsType.BrandBannerSprite);
                logo.sprite = MonetizrManager.Instance.GetAsset<Sprite>(challengeId, AssetsType.BrandLogoSprite);

                string brandTitle = MonetizrManager.Instance.GetAsset<string>(challengeId, AssetsType.BrandTitleString);

                title.text = $"{brandTitle} video";
                text.text = $"<color=#F05627>Watch video</color> by {brandTitle} to get 2 Energy Boosts";

                //buttonText.text = "Learn More";
                buttonText.text = "Got it!";

                rewardImage.gameObject.SetActive(false);
                rewardAmount.gameObject.SetActive(false);

                closeButton.onClick.AddListener(OnButtonPress);
            }
        }

        private void PrepareCongratsPanel()
        {
            var challenges = MonetizrManager.Instance.GetAvailableChallenges();

            if (challenges.Count > 0)
            {
                var challengeId = challenges[0];

                banner.sprite = MonetizrManager.Instance.GetAsset<Sprite>(challengeId, AssetsType.BrandRewardBannerSprite);
                logo.sprite = MonetizrManager.Instance.GetAsset<Sprite>(challengeId, AssetsType.BrandLogoSprite);

                string brandTitle = MonetizrManager.Instance.GetAsset<string>(challengeId, AssetsType.BrandTitleString);

                title.text = $"Congrats!";
                text.text = $"You got <color=#F05627>2 Energy Boosts</color> from {brandTitle}";

                //buttonText.text = "Learn More";
                buttonText.text = "Awesome!";

                rewardImage.gameObject.SetActive(true);
                rewardAmount.gameObject.SetActive(true);

                closeButton.onClick.AddListener(OnButtonPress);
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

        //// Start is called before the first frame update
        //void Start()
        //{

        //}

        //// Update is called once per frame
        //void Update()
        //{

        //}
    }

}