using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using Monetizr.Challenges;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

public class MissionEntry : MonoBehaviour
{
    public Text descText;

    public Text rewardText;
    public Button claimButton;
    public Text progressText;
	public Image background;
	public Image banner;

	public Color backgroundColor;
	public Color titleColor;


    public void FillWithMission(MissionBase m, MissionUI owner)
    {
        descText.text = m.GetMissionDesc();
        rewardText.text = m.reward.ToString();
		banner.enabled = false;
        
        Debug.Log($"Filling with {m.GetMissionDesc()}");

        if (m.GetType() == typeof(SponsoredPickupCoinMission))
        {
	        SponsoredPickupCoinMission sponsoredMission = (SponsoredPickupCoinMission) m;
	        if (sponsoredMission.challenge.assets.Any(asset => asset.type == "banner"))
	        {
		        Challenge.Asset bannerAsset = sponsoredMission.challenge.assets.FirstOrDefault(asset => asset.type == "banner");
		        StartCoroutine(AssetsHelper.Download2DAsset(bannerAsset, (asset, sprite) => { banner.sprite = sprite; banner.enabled = true; }));
	        }
        }

        if (m.isComplete)
        {
            claimButton.gameObject.SetActive(true);
            progressText.gameObject.SetActive(false);

			background.color = backgroundColor;

			progressText.color = Color.black;
			descText.color = titleColor;
			rewardText.color = Color.black;

			claimButton.onClick.AddListener(delegate { owner.Claim(m); } );

			if (m.GetType() != typeof(SponsoredPickupCoinMission)) return;
			
			SponsoredPickupCoinMission sponsoredMission = (SponsoredPickupCoinMission) m;
			claimButton.onClick.AddListener(delegate
			{
				//SponsoredMissionsManager.instance.ClaimReward(sponsoredMission.challenge);
			});
        }
        else
        {
            claimButton.gameObject.SetActive(false);
            progressText.gameObject.SetActive(true);

			background.color = backgroundColor;

			progressText.color = Color.black;
			descText.color = titleColor;

			progressText.text = ((int)m.progress) + " / " + ((int)m.max);
        }
    }
}
