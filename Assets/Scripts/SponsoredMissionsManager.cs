using System;
using System.Collections.Generic;
using Monetizr.Campaigns;
using UnityEngine;

public class SponsoredMissionsManager : MonoBehaviour
{
    public static SponsoredMissionsManager instance { get; private set; }

    public GameObject loadingUI = null;
    public Animator menuActors = null;
    public Animator menuEnv = null;
    public MusicPlayer musicPlayer = null;

    public Sprite defaultRewardIcon;
    public Sprite defaultRegularCoinIcon;

    [SerializeField] private string apiKey;

    

    private void Awake()
    {
        instance = this;


        if (loadingUI == null)
            return;

        loadingUI?.SetActive(true);
 

        MonetizrManager.Initialize(apiKey, (bool isOK) =>
            {
                InitializeSponsoredMissions();
            },
            (bool soundOn) => 
            {
                musicPlayer.SwitchMusic(soundOn);
            } );

        MonetizrManager.SetTeaserPosition(new Vector2(-430, 100));

        loadingUI?.SetActive(false);
        menuActors?.Play("Take 001");
        menuEnv?.Play("Take 001");

    }

    public void InitializeSponsoredMissions()
    {
        MonetizrManager.RegisterSponsoredMission(defaultRewardIcon, 2, "Gold Cans", 
            (int reward) => { PlayerData.instance.ClaimSponsoredMission(reward); } 
            
            );

        MonetizrManager.RegisterSponsoredMission2(defaultRegularCoinIcon, 200, "Fish Bones",
          () => { return PlayerData.instance.coins;  },
          (int coins) => { PlayerData.instance.AddCoins(coins); }
          );

        MonetizrManager.RegisterSponsoredMission(defaultRewardIcon, 8, "Magic Crystals",
          (int reward) => { PlayerData.instance.ClaimSponsoredMission(reward); }

          );
    }
      


   
}
