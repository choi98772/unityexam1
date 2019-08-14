using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHpBar : MonoBehaviour {
    public float mMaxWidth = 96;
    public GameObject mBar = null;

    CUnit mTarget = null;
    
    RectTransform mTransform = null;
    RectTransform mBarTransform = null;

    // Use this for initialization
    void Start () {
        mTransform = gameObject.GetComponent<RectTransform>();
        mBarTransform = mBar.GetComponent<RectTransform>();

        mTransform.SetParent(CUIManager.GetInstance().GetTransform());
    }
	
	// Update is called once per frame
	void Update () {
		if (mTarget.GetStatus() == CUnit.UNIT_STATUS.US_DYING)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 pos = mTarget.GetPos() * CGameScaller.GetInstance().GetScale();

        pos -= CGameManager.GetInstance().GetPos();
        pos.z = 1.0f;

        pos *= 1.0f / CUIManager.GetInstance().GetScaleFactor();
        pos.y += 25;

        SetPos(pos.x, pos.y);

        SetValue(mTarget.GetHpPercent());
	}

    public void SetTarget(CUnit unit)
    {
        mTarget = unit;
    }

    private void SetPos(float x, float y)
    {
        mTransform.localPosition = new Vector3(x, y, 0);
    }

    private void SetValue(float percent)
    {
        Vector2 size = mBarTransform.sizeDelta;

        size.x = percent * mMaxWidth;
        mBarTransform.sizeDelta = size;
    }
}
