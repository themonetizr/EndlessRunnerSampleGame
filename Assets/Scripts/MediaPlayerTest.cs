using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.IO;

using Monetizr.Challenges;

class CallBackTest : IMediaPlayerCallback {
	public void onVideoFinish( string data ) {
		Debug.Log("Video played, data: " + data);
		MediaPlayerTest.console.Print("Video played, data: " + data);
	}
	public void onVideoClose( string data )  {
		Debug.Log("Video closed, data: " + data);
		MediaPlayerTest.console.Print("Video closed, data: " + data);
	}
}

public class MediaPlayerTest : MonoBehaviour {
	
	CallBackTest m_callback;
    static public ConsoleManager console;


    // Use this for initialization
    void Start () {

        MonetizrManager.Initialize("PUHzF8UQLXJUuaW0vX0D0lTAFlWU2G0J2NaN2SHk6AA", LoadingVideoAsset);

		console = ConsoleManager.Initialize(true);
       
	}

    void LoadingVideoAsset(bool isOK)
    {
        MonetizrManager.ShowStartupNotification(null);

        /*byte[] data = await Monetizr.Challenges.DownloadHelper.DownloadAssetData("https://www.learningcontainer.com/wp-content/uploads/2020/05/sample-mp4-file.mp4", () => { });

		string path = Application.persistentDataPath + "/challenge_id";
		string fpath = path + "/test.mp4";

		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);

		if(!File.Exists(fpath)) 
			File.WriteAllBytes(fpath, data);

		Debug.Log("Loading complete to: " + fpath);

		console.Print("Loading complete to: " + fpath);


		PlayVideo(fpath);*/
    }

	void PlayVideo(string fpath)
    {
		m_callback = new CallBackTest();
		MediaPlayerBehavior player = GetComponent<MediaPlayerBehavior>();
		
		if( player != null )
		{
			console.Print("Init player video with: " + fpath);

			player.Init( "Test title", fpath);
			player.setCallback( m_callback );
		}
    }
}
