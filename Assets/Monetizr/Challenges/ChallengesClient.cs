using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

            Debug.Log(apiKey);
        }

        [Serializable]
        private class Challenges
        {
            public Challenge[] challenges;
        }

        public async Task<Challenge[]> GetList()
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
            Debug.Log(challengesString);

            if (!response.IsSuccessStatusCode) return new Challenge[0];

            var challenges = JsonUtility.FromJson<Challenges>("{\"challenges\":" + challengesString + "}");
            return challenges.challenges;
        }
        
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
            public double progress;
        }

        public async Task UpdateStatus(Challenge challenge, int progress)
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
            
            await Client.SendAsync(requestMessage);
        }
        
        public async Task Claim(Challenge challenge)
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
            
            await Client.SendAsync(requestMessage);
        }
    }
}