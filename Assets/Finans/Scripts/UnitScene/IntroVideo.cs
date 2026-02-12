using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using UnityEngine;
using UnityEngine.Video;
//using YoutubePlayer.Components;

[RequireComponent(typeof(CurrentSceneName))]
public class IntroVideo : MonoBehaviour
{
  [SerializeField]
  GameObject loadingActivity;
  [SerializeField] GameObject youtubePlayer;

  //VideoPlayer videoplayer;
  // InvidiousVideoPlayer invidiousVideoplayer;
  // bool videoplayerEnabled = false;
  // Start is called before the first frame update
  void Start()
  {
    // videoplayer = youtubePlayer.GetComponent<VideoPlayer>();
    // invidiousVideoplayer = youtubePlayer.GetComponent<InvidiousVideoPlayer>();

    // Debug.Log($"Found {invidiousVideoplayer.VideoId}");

    //  invidiousVideoplayer.enabled = true;
  }
  public string YouTubeVideoIdFromUrl(string url)
  {
    var uri = new Uri(url);
    /*     var query = HttpUtility.ParseQueryString(uri.Query);
        if (query.AllKeys.Contains("v"))
        {
          return query["v"];
        }
        return uri.Segments.Last();
      } */
    return uri.ToString();
  }
  private const string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
  private static Regex regexExtractId = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);
  private static string[] validAuthorities = { "youtube.com", "www.youtube.com", "youtu.be", "www.youtu.be" };

  public string ExtractVideoIdFromUri(Uri uri)
  {
    try
    {
      string authority = new UriBuilder(uri).Uri.Authority.ToLower();

      //check if the url is a youtube url
      if (validAuthorities.Contains(authority))
      {
        //and extract the id
        var regRes = regexExtractId.Match(uri.ToString());
        if (regRes.Success)
        {
          return regRes.Groups[1].Value;
        }
      }
    }
    catch { }


    return null;
  }


  // Update is called once per frame
  /*void Update()
  {
    if (!videoplayerEnabled && invidiousVideoplayer.videoLoaded)
    {
      videoplayerEnabled = true;
      loadingActivity.SetActive(false);
      //videoplayer.enabled = true;
    }
  }*/
}
