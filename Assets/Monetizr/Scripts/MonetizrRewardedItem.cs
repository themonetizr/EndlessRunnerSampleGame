using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class MonetizrRewardedItem : MonoBehaviour
    {
        public Image banner;
        public Image brandIcon;
        public Text rewardTitle;
        public Text rewardDescription;
        public ButtonController actionButton;
        public Text boosterNumber;
        public Image boosterIcon;
        public GameObject progressBar;
        public Image rewardLine;
        public Text rewardPercent;
        public Text buttonText;

        public Sprite defaultBoosterIcon;

        public Image backgroundImage;

        internal void UpdateWithDescription(RewardCenterPanel rewardCenterPanel, MissionUIDescription md)
        {
            banner.gameObject.SetActive(md.brandBanner != null);
            banner.sprite = md.brandBanner;

            //not sponsored
            if (md.brandBanner == null)
            {
                var rect = GetComponent<RectTransform>();

                rect.sizeDelta = new Vector2(1010,410);

                buttonText.text = "Claim reward";
            }
            else
            {                
                buttonText.text = md.claimButtonText;
            }


            brandIcon.sprite = md.missionIcon;

            rewardTitle.text = md.missionTitle;

            rewardDescription.text = md.missionDescription;

            var ch = MonetizrManager.Instance.GetActiveChallenge();

            if (ch != null)
            {

                var color = MonetizrManager.Instance.GetAsset<Color>(ch, AssetsType.CampaignHeaderTextColor);

                if (color != default(Color))
                    rewardTitle.color = color;

                color = MonetizrManager.Instance.GetAsset<Color>(ch, AssetsType.CampaignTextColor);

                if (color != default(Color))
                    rewardDescription.color = color;

                color = MonetizrManager.Instance.GetAsset<Color>(ch, AssetsType.CampaignBackgroundColor);

                if (color != default(Color))
                    backgroundImage.color = color;

            }



            actionButton.clickReceiver = rewardCenterPanel;
            actionButton.missionDescription = md;
            //actionButton.onClick.AddListener( ()=> { md.onClaimButtonPress.Invoke(); });

            boosterNumber.text = md.reward.ToString();

            boosterIcon.sprite = md.rewardIcon == null ? defaultBoosterIcon : md.rewardIcon;

            rewardLine.fillAmount = md.progress;

            rewardPercent.text = $"{md.progress*100.0f:F1}%";

            if(md.progress < 1.0f) //reward isn't completed
            {
                progressBar.SetActive(true);
                actionButton.gameObject.SetActive(false);
            }
            else
            {
                progressBar.SetActive(false);
                actionButton.gameObject.SetActive(true);
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