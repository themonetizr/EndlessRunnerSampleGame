using System;
using System.Collections.Generic;
using Monetizr.Challenges;
using UnityEngine;

public class SponsoredMissionsManager : MonoBehaviour
{
    public static SponsoredMissionsManager instance { get; private set; }

    public GameObject loadingUI = null;
    public Animator menuActors = null;
    public Animator menuEnv = null;
    public MusicPlayer musicPlayer = null;

    public Sprite defaultRewardIcon;

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


        loadingUI?.SetActive(false);
        menuActors?.Play("Take 001");
        menuEnv?.Play("Take 001");

    }

    public void InitializeSponsoredMissions()
    {
        MonetizrManager.RegisterSponsoredMission(defaultRewardIcon, 2, "Enegry Boosters", 
            (int reward) => { PlayerData.instance.ClaimSponsoredMission(reward); } 
            
            );

        MonetizrManager.RegisterSponsoredMission(defaultRewardIcon, 4, "Super Coins",
          (int reward) => { PlayerData.instance.ClaimSponsoredMission(reward); }

          );

        MonetizrManager.RegisterSponsoredMission(defaultRewardIcon, 8, "Magic Crystals",
          (int reward) => { PlayerData.instance.ClaimSponsoredMission(reward); }

          );
    }
      


   
}
