using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
        }

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

            return !response.IsSuccessStatusCode ? new List<Challenge>() : JsonConvert.DeserializeObject<List<Challenge>>(await response.Content.ReadAsStringAsync());
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

            return !response.IsSuccessStatusCode ? null : JsonConvert.DeserializeObject<Challenge>(await response.Content.ReadAsStringAsync());
        }

        public async Task UpdateStatus(Challenge challenge, int progress)
        {
            var requestBody = new
            {
               progress = Mathf.Clamp(progress, 0, 100).ToString()
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
                    JsonConvert.SerializeObject(requestBody), 
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