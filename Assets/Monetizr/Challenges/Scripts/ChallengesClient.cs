using System;
using System.Collections.Generic;

// This does not work in webgl
using System.Net.Http;
using System.Net.Http.Headers;

using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using System.Threading.Tasks;
using UnityEngine;
using Monetizr.Challenges.Analytics;

public class UnityWebRequestAwaiter : INotifyCompletion
{
	private UnityWebRequestAsyncOperation asyncOp;
	private Action continuation;

	public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
	{
		this.asyncOp = asyncOp;
		asyncOp.completed += OnRequestCompleted;
	}

	public bool IsCompleted { get { return asyncOp.isDone; } }

	public void GetResult() { }

	public void OnCompleted(Action continuation)
	{
		this.continuation = continuation;
	}

	private void OnRequestCompleted(AsyncOperation obj)
	{
		continuation();
	}
}

public static class ExtensionMethods
{
	public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
	{
		return new UnityWebRequestAwaiter(asyncOp);
	}
}

namespace Monetizr.Challenges
{
    
    public class ChallengesClient
    {
        public PlayerInfo playerInfo { get; set; }

        private const string k_BaseUri = "https://api3.themonetizr.com/";
        private static readonly HttpClient Client = new HttpClient();
	    private static string apiBearer = "";

        public ChallengesClient(string apiKey, int timeout = 30)
        {
            Client.Timeout = TimeSpan.FromSeconds(timeout);
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            Client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
	        apiBearer = apiKey;
        }

	    private UnityWebRequest GetWebClient(string actionUrl, string method)
        {
            string finalUrl = k_BaseUri + actionUrl;
            var client = new UnityWebRequest(finalUrl, method);
            client.SetRequestHeader("Content-Type", "application/json");
            client.SetRequestHeader("Accept", "application/json");
            client.SetRequestHeader("Authorization", "Bearer " + apiBearer);
            client.SetRequestHeader("location", playerInfo.location);
            client.SetRequestHeader("age", playerInfo.age.ToString());
            client.SetRequestHeader("game-type", playerInfo.gameType);
            client.SetRequestHeader("player-id", playerInfo.playerId);
            client.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            client.timeout = 20;
            return client;
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
            string finalUrl = "api/challenges";
            UnityWebRequest request = GetWebClient(finalUrl, "GET");

            await request.SendWebRequest();


            if (request.result != UnityWebRequest.Result.Success) {
                Debug.Log($"{request.error}");
                return new List<Challenge>();
                
            } else {
                var challengesString = request.downloadHandler.text;

                var challenges = JsonUtility.FromJson<Challenges>("{\"challenges\":" + challengesString + "}");

                ChallengeAnalytics.Update(new List<Challenge>(challenges.challenges));
                
                return new List<Challenge>(challenges.challenges);
            }
        }


        /// <summary>
        /// Returns a single challenge that matches the ID.
        /// </summary>
        public async Task<Challenge> GetSingle(string id)
        {
            string finalUrl = "api/challenges" + id;
            UnityWebRequest request = GetWebClient(finalUrl, "GET");

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.Log($"{request.error}");
                return null;
            } else {
                var challengesString = request.downloadHandler.text;
                return JsonUtility.FromJson<Challenge>(challengesString);
            }
        }

        [Serializable]
        private class Status
        {
            public int progress;
        }

        [Serializable]
        private class Claimable
        {
            public string duration;
        }

        /// <summary>
        /// Updates challenge progress to a given value (in range 0 - 100)
        /// </summary>
        public async Task UpdateStatus(Challenge challenge, int progress, Action onSuccess = null, Action onFailure = null)
        {
            var status = new Status
            {
                progress = Mathf.Clamp(progress, 0, 100)
            };


            string finalUrl = "api/challenges/" + challenge.id + "/status";
            UnityWebRequest request = GetWebClient(finalUrl, "POST");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(status));
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Monetizr status failure {request.error}");
                onFailure.Invoke();
            }
            else
            {
                ChallengeAnalytics.MarkChallengeStatusUpdate(challenge);
                challenge.progress = progress;
                onSuccess?.Invoke();
            }
        }

        /// <summary>
        /// Marks the challenge as claimed by the player.
        /// </summary>
        public async Task Claim(Challenge challenge, Action onSuccess = null, Action onFailure = null)
        {

            var status = new Claimable
            {
                duration = ChallengeAnalytics.GetElapsedTime(challenge).ToString()
            };

            string finalUrl = "api/challenges/" + challenge.id + "/claim";
            UnityWebRequest request = GetWebClient(finalUrl, "POST");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(status));
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Monetizr claim failure {request.error}");
                onFailure.Invoke();
            }
            else
            {
                onSuccess?.Invoke();
            }

            // HttpRequestMessage requestMessage = new HttpRequestMessage
            // {
            //     Method = HttpMethod.Post,
            //     RequestUri = new Uri(k_BaseUri + "api/challenges/" + challenge.id + "/claim"),
            //     Headers =
            //     {
            //         {"location", playerInfo.location},
            //         {"age", playerInfo.age.ToString()},
            //         {"game-type", playerInfo.gameType},
            //         {"player-id", playerInfo.playerId},
            //         {"duration", ChallengeAnalytics.GetElapsedTime(challenge).ToString()}
            //     }
            // };

            // HttpResponseMessage response = await Client.SendAsync(requestMessage);

            // if (response.IsSuccessStatusCode)
            // {
            //     onSuccess?.Invoke();
            // }
            // else
            // {
            //     onFailure?.Invoke();
            // }
        }
    }
}
