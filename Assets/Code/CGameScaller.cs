using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGameScaller : MonoBehaviour {
    static CGameScaller mInstance = null;

    public static CGameScaller GetInstance()
    {
        return mInstance;
    }

    public float mDesignWidth = 960;
    public float mDesignHeight = 600;

    float mScale = 1.0f;
    float mScaleRev = 1.0f;

    // Use this for initialization
    void Start () {
        ScaleBg();
        mInstance = this;
    }

    private void ScaleBg()
    {
        float widthBaseScale = Screen.width / mDesignWidth;
        float heightBaseScale = Screen.height / mDesignHeight;

        float heightBaseWidth = heightBaseScale * mDesignWidth;

        mScale = 1.0f;
        mScaleRev = 1.0f;

        if (heightBaseWidth > Screen.width)
        {
            mScale = widthBaseScale;
        }
        else
        {
            mScale = heightBaseScale;
        }

        mScaleRev = 1.0f / mScale;

        transform.localScale = new Vector3(mScale, mScale, 1.0f);
    }

    public float GetScale()
    {
        return mScale;
    }

    public float GetScaleRev()
    {
        return mScaleRev;
    }
}
