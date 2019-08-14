using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEffect : MonoBehaviour {

    public void SetPos(float x, float y)
    {
        gameObject.transform.position = new Vector3(x, y, 0);
    }

    public void OnAnimationCompleted()
    {
        Destroy(gameObject);
    }
}
