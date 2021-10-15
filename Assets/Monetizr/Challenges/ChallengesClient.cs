using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Monetizr.Challenges.Analytics;

namespace Monetizr.Challenges
{
    public class ChallengesClient
    {
        public PlayerInfo playerInfo { get; set; }
        
        private const string k_BaseUri = "https://api3.themonetizr.com/";
        private static readonly HttpClient Client = new HttpClient();

        public ChallengesClient(string apiKey, int timeout = 30)
        {
            Client.Timeout = TimeSpan.FromSeconds(timeout);
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            Client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [Serializable]
        private class Challenges
        {
            public Challenge[] challenges;
        }

        /// <summary>
        /// Returns a list of challenges available to the player.
        /// </summary>
        public async Task<List<Challenge>> GetList()
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(k_BaseUri + "api/challenges"),
                Headers =
                {
                    {"location", playerInfo.location},
                    {"age", playerInfo.age.ToString()},
                    {"game-type", playerInfo.gameType},
                    {"player-id", playerInfo.playerId},
                }
            };

            HttpResponseMessage response = await Client.SendAsync(requestMessage);

            var challengesString = await response.Content.ReadAsStringAsync();

            if(response.IsSuccessStatusCode)
            {
                var challenges = JsonUtility.FromJson<Challenges>("{\"challenges\":" + challengesString + "}");

                ChallengeAnalytics.Update(new List<Challenge>(challenges.challenges));

                return new List<Challenge>(challenges.challenges);
            }
            else
            {
                return new List<Challenge>();
            }
            
        }

        /// <summary>
        /// Returns a single challenge that matches the ID.
        /// </summary>
        public async Task<Challenge> GetSingle(string id)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(k_BaseUri + "api/challenges/" + id),
                Headers =
                {
                    {"location", playerInfo.location},
                    {"age", playerInfo.age.ToString()},
                    {"game-type", playerInfo.gameType},
                    {"player-id", playerInfo.playerId},
                }
            };

            HttpResponseMessage response = await Client.SendAsync(requestMessage);

            return !response.IsSuccessStatusCode ? null : JsonUtility.FromJson<Challenge>(await response.Content.ReadAsStringAsync());
        }

        [Serializable]
        private class Status
        {
            public int progress;
        }

        /// <summary>
        /// Updates challenge progress to a given value (in range 0 - 100)
        /// </summary>
        public async Task UpdateStatus(Challenge challenge, int progress, Action onSuccess = null, Action onFailure = null)
        {
            var status = new Status{
               progress = Mathf.Clamp(progress, 0, 100)
            };
            
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(k_BaseUri + "api/challenges/" + challenge.id + "/status"),
                Headers =
                {
                    {"location", playerInfo.location},
                    {"age", playerInfo.age.ToString()},
                    {"game-type", playerInfo.gameType},
                    {"player-id", playerInfo.playerId},
                },
                Content = new StringContent(
                    JsonUtility.ToJson(status), 
                    Encoding.UTF8, 
                    "application/json"
                )
            };
            
            HttpResponseMessage response = await Client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                ChallengeAnalytics.MarkChallengeStatusUpdate(challenge);
                challenge.progress = progress;
                onSuccess?.Invoke();
            }
            else
            {
                onFailure.Invoke();
            }
        }

        /// <summary>
        /// Marks the challenge as claimed by the player.
        /// </summary>
        public async Task Claim(Challenge challenge, Action onSuccess = null, Action onFailure = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(k_BaseUri + "api/challenges/" + challenge.id + "/claim"),
                Headers =
                {
                    {"location", playerInfo.location},
                    {"age", playerInfo.age.ToString()},
                    {"game-type", playerInfo.gameType},
                    {"player-id", playerInfo.playerId},
                }
            };
            
            HttpResponseMessage response = await Client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailure?.Invoke();
            }
        }
    }
}