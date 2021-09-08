using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using Monetizr.Challenges;
using UnityEngine.Networking;

public class MissionEntry : MonoBehaviour
{
    public Text descText;
    public Text rewardText;
    public Button claimButton;
    public Text progressText;
	public Image background;

	public Color notCompletedColor;
	public Color completedColor;

    public void FillWithMission(MissionBase m, MissionUI owner)
    {
        descText.text = m.GetMissionDesc();
        rewardText.text = m.reward.ToString();
        
        Debug.Log($"Filling with {m.GetMissionDesc()}");

        if (m.GetType() == typeof(SponsoredPickupCoinMission))
        {
	        SponsoredPickupCoinMission sponsoredMission = (SponsoredPickupCoinMission) m;
	        if (sponsoredMission.challenge.assets.Any(asset => asset.type == "banner"))
	        {
		        Challenge.Asset bannerAsset = sponsoredMission.challenge.assets.FirstOrDefault(asset => asset.type == "banner");
		        StartCoroutine(AssetsHelper.DownloadAsset(bannerAsset, (asset, sprite) => background.sprite = sprite));
	        }
        }

        if (m.isComplete)
        {
            claimButton.gameObject.SetActive(true);
            progressText.gameObject.SetActive(false);

			//background.color = completedColor;

			progressText.color = Color.white;
			descText.color = Color.white;
			rewardText.color = Color.white;

			claimButton.onClick.AddListener(delegate { owner.Claim(m); } );

			if (m.GetType() != typeof(SponsoredPickupCoinMission)) return;
			
			SponsoredPickupCoinMission sponsoredMission = (SponsoredPickupCoinMission) m;
			claimButton.onClick.AddListener(delegate
			{
				SponsoredMissionsManager.instance.ClaimReward(sponsoredMission.challenge);
			});
        }
        else
        {
            claimButton.gameObject.SetActive(false);
            progressText.gameObject.SetActive(true);

			background.color = notCompletedColor;

			progressText.color = Color.black;
			descText.color = completedColor;

			progressText.text = ((int)m.progress) + " / " + ((int)m.max);
        }
    }
}
