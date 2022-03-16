using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Monetizr.Challenges;

#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

public class StartButton : MonoBehaviour
{
    private UniWebView webView = null;
    private string webUrl;

    public void SurveyLink()
    {
        //webView = gameObject.AddComponent<UniWebView>();

        //var w = Screen.width;
        //var h = Screen.width * 1.5f;
        //var x = 0;
        //var y = (Screen.height - h) / 2;

        //webView.Frame = new Rect(x, y, w, h);

        //var page = MonetizrManager.Instance.GetAsset<string>(MonetizrManager.Instance.GetAvailableChallenges()[0], AssetsType.SurveyURLString);

        //webUrl = page;
        //webView.Load(page);

        ////webView.LoadHTMLString("<p>Hello World</p>", "https://domain.com");


        //webView.Show();

        MonetizrManager.ShowSurvey(null);

    }

    //private void Update()
    //{
    //    if(webView != null)
    //    {
    //        var currentUrl = webView.Url;

    //        if (!webUrl.Equals(currentUrl))
    //        {
    //            webUrl = currentUrl;
    //            Debug.Log("Update: " + webView.Url);

    //            if (webUrl.Contains("https://www.pollfish.com/lp/withdraw-consent") ||
    //                webUrl.Contains("app.themonetizr.com") ||
    //                webUrl.Contains("uniwebview"))
    //            {
    //                Destroy(webView);
    //                webView = null;
    //            }

    //        }
    //   }
    //}

    public void PlayVideo()
    {
        var videoPath = MonetizrManager.Instance.GetAsset<string>(MonetizrManager.Instance.GetActiveChallenge(), AssetsType.VideoFilePathString);

        /*var webView = gameObject.AddComponent<UniWebView>();
        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);

        var page = MonetizrManager.Instance.GetAsset<string>(MonetizrManager.Instance.GetAvailableChallenges()[0], AssetsType.VideoFilePathString);

        // Load a URL.
        //webView.Load(page);

        webView.LoadHTMLString($"<video autoplay>< source src = \"{page}\" type = \"video/mp4\"/></video>",
                                "https://domain.com");

        // Show it.
        webView.Show();*/

        var videoPlayer = Camera.main.gameObject.GetComponent<UnityEngine.Video.VideoPlayer>();

        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;
        //videoPlayer.targetCameraAlpha = 0.5F;
        videoPlayer.url = videoPath;
        videoPlayer.frame = 100;
        videoPlayer.isLooping = true;


        //videoPlayer.loopPointReached += EndReached;

        videoPlayer.Play();


    }


    public void StartGame()
    {
        

        MonetizrManager.ShowStartupNotification(() =>
        {
            if (PlayerData.instance.ftueLevel == 0)
            {
                PlayerData.instance.ftueLevel = 1;
                PlayerData.instance.Save();
    #if UNITY_ANALYTICS
                AnalyticsEvent.FirstInteraction("start_button_pressed");
    #endif
            }

    #if UNITY_PURCHASING
            var module = StandardPurchasingModule.Instance();
    #endif
            SceneManager.LoadScene("main");
        });
    }

}
