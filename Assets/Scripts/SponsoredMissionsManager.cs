using System;
using System.Collections.Generic;
using Monetizr.Challenges;
using UnityEngine;

public class SponsoredMissionsManager : MonoBehaviour
    {
        public static SponsoredMissionsManager instance { get; private set; }

        [SerializeField] private string apiKey;

        private List<Challenge> challenges = new List<Challenge>();
        
        private ChallengesClient _challengesClient;
        
        private string city = "Riga";

        private int age = 29;

        private string gameType = "action";

        private string playerid = "monetizr_mj";

        private bool turnedOn = true;

        private static Dictionary<string, SponsoredPickupCoinMission> missions = new Dictionary<string, SponsoredPickupCoinMission>();

        private void Awake()
        {
            instance = this;
            
            _challengesClient = new ChallengesClient(apiKey)
            {
                playerInfo = new PlayerInfo(city, age, gameType, playerid)
            };
        }

        public async void AddSponsoredMissions()
        {
            try
            {
                challenges = await _challengesClient.GetList();

                for (int i = 0; i < challenges.Count; i++)
                {
                    SponsoredPickupCoinMission newMission = new SponsoredPickupCoinMission();
                    newMission.Created(challenges[i]);
                    missions.Add(i.ToString(), newMission);
                    PlayerData.instance.AddMission(newMission, i);
                }
            } catch (Exception e) {
                Debug.Log($"Monetizr An error occured in adding: {e.Message}");
            }
        }

        public async void UpdateMission(Challenge challenge, int progress)
        {
            try
            {
                await _challengesClient.UpdateStatus(challenge, progress);
            } catch (Exception e) {
                Debug.Log($" Monetizr An error occured: {e.Message}");
            }
        }

        public async void ClaimReward(Challenge challenge)
        {
            try
            {
                await _challengesClient.Claim(challenge);
            } catch (Exception e) {
                Debug.Log($"An error occured: {e.Message}");
            }
        }

        public void changeAPIKey(string apiKeyValue)
        {
            apiKey = apiKeyValue;
            _challengesClient = new ChallengesClient(apiKey)
            {
                playerInfo = new PlayerInfo(city, age, gameType, playerid)
            };

            if (turnedOn) {
                RemoveMissions();
                AddSponsoredMissions();
            }
        }

        private void RemoveMissions(){
            
            foreach(var mission in missions)
            {
                PlayerData.instance.RemoveMission(mission.Value);
            }

            missions = new Dictionary<string, SponsoredPickupCoinMission>();
        }

        public void changeOnOff(bool toggleOn) {
            turnedOn = toggleOn;

            if (toggleOn) {
                // Turned on
                AddSponsoredMissions();
            } else {
                // Turned off
                RemoveMissions();
            }
        }
    }
