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

    [SerializeField] private string apiKey;

    /*private List<Challenge> challenges = new List<Challenge>();

    private ChallengesClient _challengesClient;

    private string city = "Riga";

    private int age = 29;

    private string gameType = "action";

    private string playerid = "monetizr_
    mj";*/

    private bool turnedOn = true;

    private static Dictionary<string, SponsoredPickupCoinMission> missions = new Dictionary<string, SponsoredPickupCoinMission>();

    private void Awake()
    {
        instance = this;


        if (loadingUI == null)
            return;

        loadingUI?.SetActive(true);
       
      
        /*_challengesClient = new ChallengesClient(apiKey)
        {
            playerInfo = new PlayerInfo(city, age, gameType, playerid)
        };*/

        MonetizrManager.Initialize(apiKey, (bool isOK) =>
            {
                loadingUI?.SetActive(false);
                menuActors?.Play("Take 001");
                menuEnv?.Play("Take 001");
            });

    }

    public void AddSponsoredMissions()
    {
        try
        {
            //challenges = await _challengesClient.GetList();

            //for (int i = 0; i < challenges.Count; i++)
            //{
            //    SponsoredPickupCoinMission newMission = new SponsoredPickupCoinMission();
            //    newMission.Created(challenges[i]);
            //    missions.Add(i.ToString(), newMission);
            //    PlayerData.instance.AddMission(newMission, i);
            //}

            var challangesList = MonetizrManager.Instance.GetAvailableChallenges();

            for(int i = 0; i < challangesList.Count; i++)
            {
                SponsoredPickupCoinMission newMission = new SponsoredPickupCoinMission();

                newMission.Created(MonetizrManager.Instance.GetChallenge(challangesList[i]));

                missions.Add(i.ToString(), newMission);
                PlayerData.instance.AddMission(newMission, i);
            }

        }
        catch (Exception e)
        {
            Debug.Log($"Monetizr An error occured in adding: {e.Message}");
        }
    }

    public void UpdateMission(Challenge challenge, int progress)
    {
        /*try
        {
            await _challengesClient.UpdateStatus(challenge, progress);
        }
        catch (Exception e)
        {
            Debug.Log($" Monetizr An error occured: {e.Message}");
        }*/
    }

    public void ClaimReward(Challenge challenge)
    {
        //try
        //{
        //    await _challengesClient.Claim(challenge);
        //}
        //catch (Exception e)
        //{
        //    Debug.Log($"An error occured: {e.Message}");
        //}
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

    private void RemoveMissions()
    {

        foreach (var mission in missions)
        {
            PlayerData.instance.RemoveMission(mission.Value);
        }

        missions = new Dictionary<string, SponsoredPickupCoinMission>();
    }

    public void changeOnOff(bool toggleOn)
    {
        turnedOn = toggleOn;

        if (toggleOn)
        {
            // Turned on
            AddSponsoredMissions();
        }
        else
        {
            // Turned off
            RemoveMissions();
        }
    }
}
