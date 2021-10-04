using System;
using System.Collections;
using Monetizr.Challenges;
using UnityEngine;
using UnityEngine.Networking;

public static class AssetsHelper
{
    public static IEnumerator DownloadAsset(Challenge.Asset asset, Action<Challenge.Asset, Sprite> onAssetDownloaded)
    {
        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(asset.url);
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
