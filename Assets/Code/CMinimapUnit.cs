using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CMinimapUnit : MonoBehaviour {
    CUnit mTarget = null;
    RectTransform mTransform = null;
    RectTransform mParent = null;
    Image mImage = null;
    bool mIsEnabled = true;

    // Use this for initialization
    void Start () {
        mTransform = gameObject.GetComponent<RectTransform>();

        mTransform.SetParent(mParent);

        mImage = gameObject.GetComponent<Image>();
    }
	
	// Update is called once per frame
	void Update () {
        if (mTarget == null)
            return;

        if (!mTarget.IsLive())
        {
            mTarget = null;
            Destroy(gameObject);
            return;
        }

        Vector3 pos = mTarget.GetPos() * CGameScaller.GetInstance().GetScale();// - CGameManager.GetInstance().GetPos();

        pos -= CGameManager.GetInstance().GetPos();

        pos.x *= 1.0f / 20.0f;
        pos.y *= 1.0f / 20.0f;

        if (pos.x < -50.0f || pos.x > 50.0f || pos.y < -50.0f || pos.y > 50.0f)
        {
            EnableImage(false);
        }
        else
        {
            SetPos(pos.x, pos.y);
            EnableImage(true);            
        }
    }

    public void Init(CUnit unit, RectTransform parent)
    {
        mTarget = unit;
        mParent = parent;
    }

    public void EnableImage(bool isEnabled)
    {
        if (mIsEnabled == isEnabled)
            return;

        mIsEnabled = isEnabled;

        if (mImage != null)
            mImage.enabled = isEnabled;
    }

    public void SetPos(float x, float y)
    {
        mTransform.localPosition = new Vector3(x, y, 0);
    }
}
