using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CUIManager : MonoBehaviour {
    private static CUIManager mInstance = null;

    public static CUIManager GetInstance()
    {
        return mInstance;
    }
    //-----------------------------------------------
    public enum SCALE_TYPE
    {
        ST_NOSCALE,
        ST_WIDTH,
        ST_HEIGHT,
    }

    public GameObject mHpBarPrefab = null;
    public GameObject mMinimapPlayer = null;
    public GameObject mMinimapEnemy = null;

    public float mDesignWidth = 960;
    public float mDesignHeight = 600;
    private float mScaleFactor = 1.0f;
    private SCALE_TYPE mScaleType;
    private RectTransform mTransform = null;
    private CanvasScaler mScaler = null;
    private CMinimap mMinimap = null;
    private GameObject mExitButton = null;
    
	// Use this for initialization
	void Start () {
        mInstance = this;

        mTransform = gameObject.GetComponent<RectTransform>();
        mScaler = gameObject.GetComponent<CanvasScaler>();
        mMinimap = mTransform.GetComponentInChildren<CMinimap>();

        if (mMinimap == null)
        {
            Debug.Log("minimap controller not found.");
        }

        mExitButton = mTransform.Find("ExitButton").gameObject;
        mExitButton.SetActive(false);

        ScaleUI();
        Debug.Log(string.Format("design info : {0} X {1}, scale = {2}, scaletype = {3}", GetDesignWidth(), GetDesignHeight(), GetScaleFactor(), mScaleType));
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public RectTransform GetTransform()
    {
        return mTransform;
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

    public float GetDesignWidth()
    {
        return mDesignWidth;
    }

    public float GetDesignHeight()
    {
        return mDesignHeight;
    }

    public float GetScaleFactor()
    {
        return mScaleFactor;
    }

    public void OnUnitCreated(CUnit unit)
    {
        switch(unit.GetUnitType())
        {
            case UNIT_TYPE.PLAYER:
                {
                    OnPlayerCreated(unit);
                }
                break;
            case UNIT_TYPE.ENEMY:
                {
                    OnEnemyCreated(unit);
                }
                break;
        }
    }

    private void OnPlayerCreated(CUnit unit)
    {
        GameObject hpBar = Instantiate(mHpBarPrefab);

        CHpBar bar = hpBar.GetComponent<CHpBar>();
        bar.SetTarget(unit);

        mMinimap.OnUnitCreated(unit);
    }

    private void OnEnemyCreated(CUnit unit)
    {
        GameObject hpBar = Instantiate(mHpBarPrefab);

        CHpBar bar = hpBar.GetComponent<CHpBar>();
        bar.SetTarget(unit);

        mMinimap.OnUnitCreated(unit);
    }

    public void OnUnitDestroyed(CUnit unit)
    {
        switch(unit.GetUnitType())
        {
            case UNIT_TYPE.PLAYER:
            case UNIT_TYPE.ENEMY:
                mExitButton.SetActive(true);
                break;
        }
    }

    public void OnClickExit()
    {
        SceneManager.LoadScene("Intro");
    }
}
