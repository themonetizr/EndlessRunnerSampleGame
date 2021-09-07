using System;
using System.Collections.Generic;
using Monetizr.Challenges;
using UnityEngine;

public class SponsoredMissionsManager : MonoBehaviour
    {
        public static SponsoredMissionsManager instance { get; private set; }

        [SerializeField] private string apiKey;

        public ChallengesClient ChallengesClient;
        
        [SerializeField] private List<Challenge> challenges = new List<Challenge>();
        
        private void Awake()
        {
            instance = this;
            
            ChallengesClient = new ChallengesClient(apiKey)
            {
                playerInfo = new PlayerInfo("Riga", 29, "action", "janis_999")
            };
        }

        private void Start() => AddSponsoredMissions();
        
        private async void AddSponsoredMissions()
        {
            try
            {
                challenges = await ChallengesClient.GetList();
                
                for (int i = 0; i < challenges.Count; i++)
                {
                    SponsoredPickupCoinMission newMission = new SponsoredPickupCoinMission();
                    newMission.Created(challenges[i]);
                    PlayerData.instance.missions.Insert(i, newMission);
                }
            } catch (Exception e) {
                Debug.Log($"An error occured: {e.Message}");
            }
        }

        public async void UpdateMission(Challenge challenge, int progress)
        {
            try
            {
                await ChallengesClient.UpdateStatus(challenge, progress);
            } catch (Exception e) {
                Debug.Log($"An error occured: {e.Message}");
            }
        }

        public async void ClaimReward(Challenge challenge)
        {
            try
            {
                await ChallengesClient.Claim(challenge);
            } catch (Exception e) {
                Debug.Log($"An error occured: {e.Message}");
            }
        }
    }