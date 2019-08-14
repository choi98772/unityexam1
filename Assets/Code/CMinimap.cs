using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMinimap : MonoBehaviour {
    RectTransform mTransform = null;
    public GameObject mPlayerPrefab = null;
    public GameObject mEnemyPrefab = null;

    // Use this for initialization
    void Start () {
        mTransform = gameObject.GetComponent<RectTransform>();
        //mTransform.SetSiblingIndex();

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetPos(float x, float y)
    {
        mTransform.localPosition = new Vector3(x, y, 0);
    }

    public void SetSize(float w, float h)
    {
        mTransform.sizeDelta = new Vector2(w, h);
    }

    public void SetWidth(float w)
    {
        Vector2 size = mTransform.sizeDelta;

        size.x = w;

        mTransform.sizeDelta = size;
    }

    public void SetHeight(float h)
    {
        Vector2 size = mTransform.sizeDelta;

        size.y = h;

        mTransform.sizeDelta = size;
    }

    public void OnUnitCreated(CUnit unit)
    {
        GameObject prefabBase = null;

        switch(unit.GetUnitType())
        {
            case UNIT_TYPE.PLAYER:
                prefabBase = mPlayerPrefab;
                break;
            default:
                prefabBase = mEnemyPrefab;
                break;
        }

        GameObject obj = Instantiate(prefabBase);

        CMinimapUnit minimapUnit = obj.GetComponent<CMinimapUnit>();

        minimapUnit.Init(unit, mTransform);
    }
}
