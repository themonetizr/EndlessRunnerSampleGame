using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Challenges
{

    public class SurveyPanel : PanelController
    {
        public Button closeButton;
        private UniWebView webView;
        private string webUrl;

        //private Action onComplete;

        internal override void PreparePanel(PanelId id, Action onComplete, MissionUIDescription m)
        {
            MonetizrManager.Analytics.TrackEvent("Survey started");

            this.onComplete = onComplete;
            this.panelId = id;

            webView = gameObject.AddComponent<UniWebView>();

            var w = Screen.width;
            var h = Screen.width * 1.5f;
            var x = 0;
            var y = (Screen.height - h) / 2;

            float aspect = (float)Screen.height / (float)Screen.width;


            if (aspect < 1.777)
            {
                h = Screen.height * 0.8f;
                y = (Screen.height - h) / 2;

            }

            webView.Frame = new Rect(x, y, w, h);


            //var page = MonetizrManager.Instance.GetAsset<string>(MonetizrManager.Instance.GetActiveChallenge(), AssetsType.SurveyURLString);

            var page = "file://" + MonetizrManager.Instance.GetAsset<string>(MonetizrManager.Instance.GetActiveChallenge(), AssetsType.Html5ZipFilePathString);



            Debug.Log($"Url to show {page}");

            webUrl = page;

            //Debug.Log(page);

            webView.OnMessageReceived += OnMessageReceived;

            webView.OnPageStarted += OnPageStarted;

            webView.OnPageFinished += OnPageFinished;

            webView.OnPageErrorReceived += OnPageErrorReceived;

            // Load a URL.
            webView.Load(page);

            //webView.LoadHTMLString("<p>Hello World</p>", "https://domain.com");

            // Show it.
            webView.Show();
        }

        void OnMessageReceived(UniWebView webView, UniWebViewMessage message)
        {
            Debug.Log($"OnMessageReceived: {message.RawMessage} {message.Args.ToString()}");
        }

        void OnPageStarted(UniWebView webView, string url)
        {
            Debug.Log($"OnPageStarted: { url} ");
        }

        void OnPageFinished(UniWebView webView, int statusCode, string url)
        {
            Debug.Log($"OnPageFinished: {url} code: {statusCode}");

            if (statusCode >= 300)
            {
                MonetizrManager.Analytics.TrackEvent("Survey error");

                OnButtonPress();
            }
        }

        void OnPageErrorReceived(UniWebView webView, int errorCode, string url)
        {
            Debug.Log($"OnPageErrorReceived: {url} code: {errorCode}");

            MonetizrManager.Analytics.TrackEvent("Survey error");

            OnButtonPress();
        }

        private void Update()
        {
            if (webView != null)
            {
                var currentUrl = webView.Url;

                if (!webUrl.Equals(currentUrl))
                {
                    webUrl = currentUrl;
                    Debug.Log("Update: " + webView.Url);

                    if (webUrl.Contains("https://www.pollfish.com/lp/withdraw-consent") ||
                        webUrl.Contains("app.themonetizr.com") ||
                        webUrl.Contains("uniwebview"))
                    {
                        MonetizrManager.Analytics.TrackEvent("Survey completed");

                        OnButtonPress();

                        //Destroy(webView);
                        //webView = null;
                    }

                }
            }
        }

        private new void Awake()
        {
            base.Awake();


        }

        public void OnButtonPress()
        {
            MonetizrManager.Analytics.TrackEvent("Survey skipped");

            Destroy(webView);
            webView = null;

            SetActive(false);
        }

        internal override void FinalizePanel(PanelId id)
        {
            
        }

        //// Start is called before the first frame update
        //void Start()
        //{

        //}

        //// Update is called once per frame
        //void Update()
        //{

        //}
    }

}