using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CIntroManager : MonoBehaviour {
    public enum SCALE_TYPE
    {
        ST_NOSCALE,
        ST_WIDTH,
        ST_HEIGHT,
    }

    public GameObject mMainCamera = null;

    public float mDesignWidth = 960;
    public float mDesignHeight = 600;
    private float mScaleFactor = 1.0f;
    private SCALE_TYPE mScaleType;
    private CanvasScaler mScaler = null;

    private bool mGameStarting = false;
    private AsyncOperation mSceneLoadingStatus = null;
    private GameObject mProgressBarBg = null;
    private RectTransform mProgressBar = null;

    // Use this for initialization
    void Start() {
        Camera camera = mMainCamera.GetComponent<Camera>();

        camera.orthographicSize = Screen.height / 2;

        mScaler = gameObject.GetComponent<CanvasScaler>();

        mProgressBarBg = transform.Find("Progress").gameObject;
        GameObject progressBar = transform.Find("Progress/Bar").gameObject;

        mProgressBar = progressBar.GetComponent<RectTransform>();

        SetProgressValue(0.0f);

        mProgressBarBg.SetActive(false);

        ScaleUI();
    }

    void Update()
    {
        if (mGameStarting)
        {
            SetProgressValue(mSceneLoadingStatus.progress);
        }
    }

    void SetProgressValue(float value)
    {
        Vector2 size = mProgressBar.sizeDelta;

        size.x = value * 396.0f;

        mProgressBar.sizeDelta = size;
    }

    public void OnClickStart()
    {
        if (mGameStarting)
            return;

        mGameStarting = true;

        Transform startButton = transform.Find("StartButton");

        startButton.gameObject.SetActive(false);

        mSceneLoadingStatus = SceneManager.LoadSceneAsync("GameScene");

        mProgressBarBg.SetActive(true);
    }

    private void ScaleUI()
    {
        float widthBaseScale = Screen.width / mDesignWidth;
        float heightBaseScale = Screen.height / mDesignHeight;

        float heightBaseWidth = heightBaseScale * mDesignWidth;

        if (heightBaseWidth > Screen.width)
        {
            mScaleType = SCALE_TYPE.ST_WIDTH;
            mScaleFactor = widthBaseScale;
            mScaler.scaleFactor = widthBaseScale;
        }
        else
        {
            mScaleType = SCALE_TYPE.ST_HEIGHT;
            mScaleFactor = heightBaseScale;
            mScaler.scaleFactor = heightBaseScale;
        }
    }
}
