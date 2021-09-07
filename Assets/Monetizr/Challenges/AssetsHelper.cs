using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Monetizr.Challenges
{
    public static class AssetsHelper
    {
        public static IEnumerator DownloadAssets(Challenge challenge, Action<Challenge.Asset, Sprite> onAssetDownloaded)
        {
            return challenge.assets.Select(asset => DownloadAsset(asset, onAssetDownloaded)).GetEnumerator();
        }

        public static IEnumerator DownloadAsset(Challenge.Asset asset, Action<Challenge.Asset, Sprite> onAssetDownloaded)
        {
            using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(asset.obj);
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(uwr.error);
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
            onAssetDownloaded?.Invoke(asset, Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero));
        }
    }
}