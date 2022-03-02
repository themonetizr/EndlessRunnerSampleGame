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
        //public PlayerInfo playerInfo { get; set; }

        private const string k_BaseUri = "https://api3.themonetizr.com/";
        private static readonly HttpClient Client = new HttpClient();

        public MonetizrAnalytics analytics { get; private set; }        

        public ChallengesClient(string apiKey, int timeout = 30)
        {
            analytics = new MonetizrAnalytics();

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
                    //{"location", playerInfo.location},
                    //{"age", playerInfo.age.ToString()},
                    //{"game-type", playerInfo.gameType},
                    {"player-id", analytics.GetUserId()},
                }
            };

            HttpResponseMessage response = await Client.SendAsync(requestMessage);

            var challengesString = await response.Content.ReadAsStringAsync();

            Debug.Log(challengesString);

            if (response.IsSuccessStatusCode)
            {
                var challenges = JsonUtility.FromJson<Challenges>("{\"challenges\":" + challengesString + "}");

                //analytics.Update(new List<Challenge>(challenges.challenges));

                return new List<Challenge>(challenges.challenges);
            }
            else
            {
                return new List<Challenge>();
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
                    //{"location", playerInfo.location},
                    //{"age", playerInfo.age.ToString()},
                    //{"game-type", playerInfo.gameType},
                    {"player-id", analytics.GetUserId()},
                    //{"duration", analytics.GetElapsedTime(challenge).ToString()}
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