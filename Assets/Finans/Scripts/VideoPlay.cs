using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlay : MonoBehaviour
{
    [SerializeField]
    VideoPlayer myVideoPlayer;

    void Start()
    {
        myVideoPlayer.loopPointReached += ChangeScene;
    }

    // Update is called once per frame
    void ChangeScene(VideoPlayer vp)
    {
        Debug.Log("video is finished");
    }
}
