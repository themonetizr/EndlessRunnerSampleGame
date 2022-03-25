using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Challenges
{

    public class WebViewPanel : PanelController
    {   
        
        public Button closeButton;
        private UniWebView webView;
        private string webUrl;
        private MissionUIDescription currentMissionDesc;
        private string eventsPrefix;

        //private Action onComplete;

        internal void PrepareWebViewComponent()
        {
            UniWebView.SetAllowAutoPlay(true);


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

            webView.OnMessageReceived += OnMessageReceived;
            webView.OnPageStarted += OnPageStarted;
            webView.OnPageFinished += OnPageFinished;
            webView.OnPageErrorReceived += OnPageErrorReceived;
        }

        internal void PrepareSurveyPanel()
        {
            MonetizrManager.Analytics.TrackEvent("Survey started", currentMissionDesc);
            webUrl = MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.SurveyURLString);
            eventsPrefix = "Survey";

            webView.Load(webUrl);

        }

        private void PrepareHtml5Panel()
        {
            webUrl = "file://" + MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.Html5PathString);
            eventsPrefix = "Html5";

            webView.Load(webUrl);
        }

        private void PrepareVideoPanel()
        {
            webUrl = "file://" + MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.VideoFilePathString);

            var htmlFile = MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.VideoFilePathString);

            var videoName = Path.GetFileName(htmlFile);

                //var page = $"<video autoplay><source src = \"{webUrl}\" type = \"video/mp4\"/></video>";

            var page = $"<video autoplay><source src = \"{videoName}\" type = \"video/mp4\"/></video>";


            htmlFile = Path.GetDirectoryName(htmlFile) + "/" + Path.GetFileNameWithoutExtension(htmlFile) + ".html";

            Debug.Log("----------------" + htmlFile);

            if (File.Exists(htmlFile))
                File.Delete(htmlFile);

            File.WriteAllBytes(htmlFile, Encoding.ASCII.GetBytes(page));
            

            webView.LoadHTMLString(htmlFile,"http://test.com");

            eventsPrefix = "WebVideo";
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, MissionUIDescription m)
        {
            this.onComplete = onComplete;
            panelId = id;
            currentMissionDesc = m;

            PrepareWebViewComponent();


            switch (id)
            {
                case PanelId.SurveyWebView: PrepareSurveyPanel(); break;
                case PanelId.VideoWebView: PrepareVideoPanel(); break;
                case PanelId.Html5WebView: PrepareHtml5Panel(); break;
            }


            // Load a URL.
            Debug.Log($"Url to show {webUrl}");
            webView.Show();
        }

       

        void OnMessageReceived(UniWebView webView, UniWebViewMessage message)
        {
            Debug.Log($"OnMessageReceived: {message.RawMessage} {message.Args.ToString()}");

            if(message.RawMessage.Contains("close"))
            {
                OnCompleteEvent();

                ClosePanel();
            }
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
                MonetizrManager.Analytics.TrackEvent($"{eventsPrefix} error", currentMissionDesc);

                ClosePanel();
            }
        }

        void OnPageErrorReceived(UniWebView webView, int errorCode, string url)
        {
            Debug.Log($"OnPageErrorReceived: {url} code: {errorCode}");

            MonetizrManager.Analytics.TrackEvent($"{eventsPrefix} error", currentMissionDesc);

            ClosePanel();
        }

        private void Update()
        {
            if (webView != null && panelId == PanelId.SurveyWebView)
            {
                var currentUrl = webView.Url;

                if (!webUrl.Equals(currentUrl))
                {
                    webUrl = currentUrl;
                    Debug.Log("Update: " + webView.Url);

                    if (webUrl.Contains("https://www.pollfish.com/lp/withdraw-consent") ||
                        webUrl.Contains("app.themonetizr.com") /*||
                        webUrl.Contains("uniwebview")*/)
                    {
                        OnCompleteEvent();

                        OnButtonPress();

                        //Destroy(webView);
                        //webView = null;
                    }

                }
            }
        }


        private void OnCompleteEvent()
        {
            MonetizrManager.Analytics.TrackEvent($"{eventsPrefix} completed",currentMissionDesc);
            isSkipped = false; 

        }

        private void ClosePanel()
        {
            Destroy(webView);
            webView = null;

            SetActive(false);
        }

        private new void Awake()
        {
            base.Awake();


        }

        public void OnButtonPress()
        {
            isSkipped = true;
            
            MonetizrManager.Analytics.TrackEvent($"{eventsPrefix} skipped", currentMissionDesc);

            ClosePanel();
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