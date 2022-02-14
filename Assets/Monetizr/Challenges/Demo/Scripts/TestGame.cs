using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using Monetizr.Challenges;

public class TestGame : MonoBehaviour
{
    [SerializeField] private string apiKey;
    [SerializeField] private List<Challenge> challenges;
    [SerializeField] private Image image;

    [SerializeField] UnityEvent OnChallengesDownloaded;

    private ChallengesClient challengesClient;


    private void Awake()
    {
        challengesClient = new ChallengesClient(apiKey)
        {
            playerInfo = new PlayerInfo("Country", 18, "action", "user")
        };
    }

    public async void GetList()
    {
        challenges = await challengesClient.GetList();
        
        if(challenges.Count > 0)
        {
            OnChallengesDownloaded.Invoke();
        }
    }

    public void UpdateTexture()
    {
        StartCoroutine(AssetsHelper.Download2DAsset(challenges[0].assets.FirstOrDefault(asset => asset.type == "icon"), (asset, sprite) => { image.sprite = sprite; }, () => Debug.Log("Asset download failed")));
    }

    public async void UpdateStatus(int index)
    {
        await challengesClient.UpdateStatus(challenges[index], 50, delegate { Debug.Log("Challenge progress updated!"); });
    }

    public async void ClaimChallenge(int index)
    {
        await challengesClient.Claim(challenges[index], delegate { Debug.Log("Challenge claimed!"); });
    }
}
