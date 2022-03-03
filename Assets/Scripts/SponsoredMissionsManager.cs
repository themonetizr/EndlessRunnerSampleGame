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

    [SerializeField] private string apiKey;
      
    private bool turnedOn = true;

    private static Dictionary<string, SponsoredPickupCoinMission> missions = new Dictionary<string, SponsoredPickupCoinMission>();

    private void Awake()
    {
        instance = this;


        if (loadingUI == null)
            return;

        loadingUI?.SetActive(true);
 

        MonetizrManager.Initialize(apiKey, (bool isOK) =>
            {
                loadingUI?.SetActive(false);
                menuActors?.Play("Take 001");
                menuEnv?.Play("Take 001");
            },
            (bool soundOn) => 
            {
                musicPlayer.SwitchMusic(soundOn);
            } );

    }

   

    public void changeAPIKey(string apiKeyValue)
    {
        //apiKey = apiKeyValue;
        //_challengesClient = new ChallengesClient(apiKey)
        //{
        //    playerInfo = new PlayerInfo(city, age, gameType, playerid)
        //};

        //if (turnedOn)
        //{
        //    RemoveMissions();
        //    AddSponsoredMissions();
        //}
    }
       

    public void changeOnOff(bool toggleOn)
    {
        turnedOn = toggleOn;

        if (toggleOn)
        {
            // Turned on
            //AddSponsoredMissions();
        }
        else
        {
            // Turned off
            //RemoveMissions();
        }
    }
}
