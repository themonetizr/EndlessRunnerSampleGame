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
        
        private void Awake()
        {
            instance = this;
            
            _challengesClient = new ChallengesClient(apiKey)
            {
                playerInfo = new PlayerInfo("Riga", 29, "action", "janis_999")
            };
        }

        public async void AddSponsoredMissions()
        {
            try
            {
                challenges = await _challengesClient.GetList();

                Debug.Log("Sponsored challenges: " + challenges.Count);

                for (int i = 0; i < challenges.Count; i++)
                {
                    SponsoredPickupCoinMission newMission = new SponsoredPickupCoinMission();
                    newMission.Created(challenges[i]);
                    PlayerData.instance.AddMission(newMission, i);
                }
            } catch (Exception e) {
                Debug.Log($"An error occured: {e.Message}");
            }
        }

        public async void UpdateMission(Challenge challenge, int progress)
        {
            try
            {
                await _challengesClient.UpdateStatus(challenge, progress);
            } catch (Exception e) {
                Debug.Log($"An error occured: {e.Message}");
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
    }