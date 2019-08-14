using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CGameManager : MonoBehaviour {
    private const int MAX_BULLET = 100;

    static CGameManager mInstance = null;

    public static CGameManager GetInstance()
    {
        return mInstance;
    }

    Transform mTransform = null;

    CPlayer mPlayer = null;
    CEnemy mEnemy = null;

    CBullet[] mPlayerBulletList = null;
    CBullet[] mEnemyBulletList = null;

    public GameObject mPlayerPrefab = null;
    public GameObject mEnemyPrefab = null;
    public GameObject mPlayerBullet = null;
    public GameObject mEnemyBullet = null;
    public GameObject mExpEffect = null;

    public GameObject mUnitRoot = null;
    private Transform mUnitRootTransform = null;

    public GameObject mTilemapBg = null;
    private Tilemap mTileMap = null;
    private CMapManager mMapManager = null;

    public Vector3 GetPos()
    {
        return mTransform.position;
    }
    //-----------------------------------------------------------------
    public GameObject GetUnitRoot() { return mUnitRoot; }

    public void OnUnitDestroyed(CUnit unit)
    {
        switch(unit.GetUnitType())
        {
            case UNIT_TYPE.PLAYER:
                {
                    mPlayer = null;

                    Debug.Log("on player destroyed");

                    CUIManager.GetInstance().OnUnitDestroyed(unit);
                }
                break;
            case UNIT_TYPE.ENEMY:
                {
                    mEnemy = null;

                    Debug.Log("on enemy destroyed");

                    CUIManager.GetInstance().OnUnitDestroyed(unit);
                }
                break;
        }
    }

    public void OnPlayerAwake(CPlayer player)
    {
        mPlayer = player;

        CUIManager.GetInstance().OnUnitCreated(player);
    }

    public CPlayer GetPlayer()
    {
        return mPlayer;
    }

    public void OnEnemyAwake(CEnemy enemy)
    {
        mEnemy = enemy;

        CUIManager.GetInstance().OnUnitCreated(enemy);
    }

    public CEnemy GetEnemy()
    {
        return mEnemy;
    }

    //unit의 적에 해당하는 캐릭터 얻기
    public CUnit GetEnemy(CUnit unit)
    {
        switch(unit.GetUnitType())
        {
            case UNIT_TYPE.PLAYER:
            case UNIT_TYPE.BULLET_PLAYER:
                return GetEnemy();

            case UNIT_TYPE.ENEMY:
            case UNIT_TYPE.BULLET_ENEMY:

                return GetPlayer();
        }

        return null;
    }

    public void SpawnPlayer(float x, float y)
    {
        GameObject obj = Instantiate(mPlayerPrefab);

        obj.transform.parent = mUnitRootTransform;

        CPlayer player = obj.GetComponent<CPlayer>();

        player.SetPos(x, y);
    }

    public void SpawnEnemy(float x, float y)
    {
        GameObject obj = Instantiate(mEnemyPrefab);

        obj.transform.parent = mUnitRootTransform;

        CEnemy enemy = obj.GetComponent<CEnemy>();

        enemy.SetPos(x, y);
    }
    //-----------------------------------------------------------------
    public void CreatePlayerBullet(float x, float y, float tx, float ty)
    {
        if (mPlayerBullet == null)
            return;

        GameObject obj = Instantiate(mPlayerBullet);

        CBullet bullet = obj.GetComponent<CBullet>();
        int index = AddPlayerBullet(bullet);

        bullet.Init(mUnitRootTransform, UNIT_TYPE.BULLET_PLAYER, index, x, y, tx, ty);        
    }

    private int AddPlayerBullet(CBullet bullet)
    {
        int index = GetEmptyPlayerBulletSlot();

        if (index < 0)
            return -1;

        mPlayerBulletList[index] = bullet;

        return index;
    }

    public void CreateEnemyBullet(float x, float y, float tx, float ty)
    {
        if (mEnemyBullet == null)
            return;

        GameObject obj = Instantiate(mEnemyBullet);

        CBullet bullet = obj.GetComponent<CBullet>();
        int index = AddEnemyBullet(bullet);

        bullet.Init(mUnitRootTransform, UNIT_TYPE.BULLET_ENEMY, index, x, y, tx, ty);
    }

    private int AddEnemyBullet(CBullet bullet)
    {
        int index = GetEmptyEnemyBulletSlot();

        if (index < 0)
            return -1;

        mEnemyBulletList[index] = bullet;

        return index;
    }

    public void RemoveBullet(int index, UNIT_TYPE type)
    {
        if (index < 0 || index >= MAX_BULLET)
            return;

        if (type == UNIT_TYPE.BULLET_PLAYER)
            mPlayerBulletList[index] = null;
        else
            mEnemyBulletList[index] = null;
    }

    public bool FindPlayerBulletInRange(Vector3 pos, float range)
    {
        for (int i = 0;i < MAX_BULLET; ++i)
        {
            if (mPlayerBulletList[i] == null)
                continue;

            if (mPlayerBulletList[i].GetDistanceSq(pos) <= (range * range))
                return true;
        }

        return false;
    }
    //-----------------------------------------------------------------
    public void CreateEffect(float x, float y)
    {
        if (mExpEffect == null)
            return;

        GameObject obj = Instantiate(mExpEffect);

        CEffect effect = obj.GetComponent<CEffect>();

        effect.SetPos(x, y);
    }
    //-----------------------------------------------------------------
    void InitBulletList()
    {
        mPlayerBulletList = new CBullet[MAX_BULLET];
        mEnemyBulletList = new CBullet[MAX_BULLET];

        for (int i = 0;i < MAX_BULLET; ++i)
        {
            mPlayerBulletList[i] = null;
            mEnemyBulletList[i] = null;
        }
    }

    int GetEmptyPlayerBulletSlot()
    {
        for (int i = 0;i < MAX_BULLET; ++i)
        {
            if (mPlayerBulletList[i] == null)
                return i;
        }

        return -1;
    }

    int GetEmptyEnemyBulletSlot()
    {
        for (int i = 0; i < MAX_BULLET; ++i)
        {
            if (mEnemyBulletList[i] == null)
                return i;
        }

        return -1;
    }
    //-----------------------------------------------------------------
    // Use this for initialization
    void Start () {
        mInstance = this;
        mTransform = gameObject.transform;

        InitBulletList();

        mUnitRootTransform = mUnitRoot.transform;

        SpawnPlayer(107, 306);
        SpawnEnemy(959, 306);

        Camera camera = gameObject.GetComponent<Camera>();

        camera.orthographicSize = Screen.height / 2;

        mTileMap = mTilemapBg.GetComponent<Tilemap>();

        mMapManager = new CMapManager();
        mMapManager.Init(mTileMap);
    }

    void OnDestroy()
    {
        mInstance = null;
    }

    // Update is called once per frame
    void Update () {
		if (mPlayer != null)
        {
            mPlayer.UpdateUnit();

            Vector3 playerPos = mPlayer.GetPos();

            playerPos *= CGameScaller.GetInstance().GetScale();
            playerPos.z = 1.0f;

            mTransform.position = playerPos;
        }

        if (mEnemy != null)
        {
            mEnemy.UpdateUnit();
        }

        for (int i = 0;i < MAX_BULLET; ++i)
        {
            if (mPlayerBulletList[i] != null)
                mPlayerBulletList[i].UpdateUnit();

            if (mEnemyBulletList[i] != null)
                mEnemyBulletList[i].UpdateUnit();
        }
    }
    //-------------------------------------------------------------------------
    public Tilemap GetTilemap()
    {
        return mTileMap;
    }

    public CMapManager GetMapManager()
    {
        return mMapManager;
    }
    //-------------------------------------------------------------------------
    public void OnUnitDying(CUnit unit)
    {
        if (mPlayerBulletList != null)
        {
            for (int i = 0;i < MAX_BULLET; ++i)
            {
                if (mPlayerBulletList[i] != null)
                    mPlayerBulletList[i].OnUnitDying(unit);
            }
        }

        if (mEnemyBulletList != null)
        {
            for (int i = 0; i < MAX_BULLET; ++i)
            {
                if (mEnemyBulletList[i] != null)
                    mEnemyBulletList[i].OnUnitDying(unit);
            }
        }
    }
}
