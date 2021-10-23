﻿using UnityEngine;
using System.Collections;
using agora_gaming_rtc;

/// <summary>
///   AudienceVC acts as an audience to view the AR Caster's feed.  The viewer
/// can draw the area of interest on the AR Caster's screen remotely.
///   This controller relies on two other Monobehavior object:
///      - ColorController: allows user to pick a color for drawing
///      - TouchWatcher: handles screen touch for drawing.
/// </summary>
public class AudienceVC : PlayerViewControllerBase
{
    ColorButtonController colorButtonController;
    AudienceTouchWatcher touchWatcher;
    MonoBehaviour monoProxy;

    IRtcEngine rtcEngine;
    int dataStreamId = 0;
    bool ViewOnly { get; set; }


    protected override string RemoteStreamTargetImage
    {
        get
        {
            return MainVideoName;
        }
    }

    public AudienceVC(bool viewOnly)
    {
        Debug.Log("viewOnlydscvdsvfdsvf");
        Debug.Log(viewOnly);
        ViewOnly = viewOnly;
    }

    public override void OnSceneLoaded() 
    {
        base.OnSceneLoaded();
        GameObject gameObject = GameObject.Find(SelfVideoName);
        Debug.Log(SelfVideoName);
        Debug.Log("dbchjdsbcjdhsbcjhsdbcjhdsbcjdhsSelfVideoName");
        if (gameObject != null)
        {
            gameObject.AddComponent<VideoSurface>();
        }

        gameObject = GameObject.Find("ColorController");
        if (gameObject != null)
        {
            colorButtonController = gameObject.GetComponent<ColorButtonController>();
            monoProxy = colorButtonController.GetComponent<MonoBehaviour>();

        }

        gameObject = GameObject.Find("TouchWatcher");
        if (gameObject != null)
        {
            Debug.Log("TouchWatcher==========>Audience");
            touchWatcher = gameObject.GetComponent<AudienceTouchWatcher>();
            Debug.Log("TouchWatcher==========>Audience");
            Debug.Log(touchWatcher);
            touchWatcher.DrawColor = colorButtonController.SelectedColor;
            touchWatcher.ProcessDrawing += ProcessDrawing;
            touchWatcher.NotifyClearDrawings += delegate ()
            {
                monoProxy.StartCoroutine(CoClearDrawing());

            };

            colorButtonController.OnColorChange += delegate (Color color)
            {
                touchWatcher.DrawColor = color;
            };
        }

        if (ViewOnly)
        {
            colorButtonController?.gameObject.SetActive(false);
            touchWatcher?.gameObject.SetActive(false);
            GameObject.Find(SelfVideoName)?.SetActive(false);
            GameObject.Find("ToggleButton")?.SetActive(false);
            GameObject.Find("ButtonClear")?.SetActive(false);
        }

        rtcEngine = IRtcEngine.QueryEngine();
        dataStreamId = rtcEngine.CreateDataStream(reliable: true, ordered: true);
    }

    public override void Join(string channel)
    {
        Debug.Log("Aud calling join (channel = " + channel + ")");

        if (mRtcEngine == null)
            return;

        // set callbacks (optional)
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccess;
        mRtcEngine.OnUserJoined = OnUserJoined;
        mRtcEngine.OnUserOffline = OnUserOffline;

        // enable video
        mRtcEngine.EnableVideo();
        // allow camera output callback
        mRtcEngine.EnableVideoObserver();
        mRtcEngine.EnableLocalAudio(false);
        mRtcEngine.MuteLocalAudioStream(true);

        if (ViewOnly)
        {
            mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
        }

        // join channel
        mRtcEngine.JoinChannel(channel, null, 0);

        Debug.Log("initializeEngine done");
    }
    void ProcessDrawing(DrawmarkModel dm)
    {
        Debug.Log("CoProcessDrawing==============>");
        monoProxy.StartCoroutine(CoProcessDrawing(dm));
    }

    IEnumerator CoProcessDrawing(DrawmarkModel dm)
    {
        string json = JsonUtility.ToJson(dm);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
        if (dataStreamId > 0)
        {
            rtcEngine.SendStreamMessage(dataStreamId, data);
        }

        yield return null;
    }

    IEnumerator CoClearDrawing()
    {
        string json = "{\"clear\": true}";
        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
        if (dataStreamId > 0)
        {
            rtcEngine.SendStreamMessage(dataStreamId, data);
        }

        yield return null;
    }
}
