using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CPlayer : CUnit
{
    private bool mLeftOn = false;
    private bool mRightOn = false;
    private bool mUpOn = false;
    private bool mDownOn = false;
    private int mMoveMask = 0;
    private float mAttackDelay = 0.0f; //공격 지연시간, 이 값은 공격후 mAttackTime 으로 설정됨
    private float mAttackTime = 0.2f;//한번의 공격이 완료되는데까지 걸리는 시간

    private float mCurrentAttackSpeed = 0.0f;
    public float mAttackSpeed = 0.6f;

    public float mCurrentBlockingTime = 0.0f;
    public float mMaxBlockingTime = 0.6f; //블럭 애니메이션 시간
    public float mBlockingRate = 70.0f; //공격을 블럭할 확률

    private Vector3 mKnockbackDirection;
    private float mKnockbackForce;
    public float mKnockbackTime = 0.0f;
    public float mKnockbackMaxTime = 0.2f;

    public float mDyingTime = 1.0f;

    private CMapManager mMapManager = null;

    public override void Initialize()
    {
        base.Initialize();
        SetUnitType(UNIT_TYPE.PLAYER);

        CGameManager.GetInstance().OnPlayerAwake(this);

        mMapManager = CGameManager.GetInstance().GetMapManager();
    }

    public override void UpdateUnit()
    {
        if (mCurrentAttackSpeed > 0.0f)
        {
            mCurrentAttackSpeed -= Time.deltaTime;
        }

        ProcessKey();

        switch (GetStatus())
        {
            case UNIT_STATUS.US_IDLE:
                break;
            case UNIT_STATUS.US_MOVE:
                MoveUnit();
                break;
            case UNIT_STATUS.US_ATTACK:
                UpdateAttack();
                break;
            case UNIT_STATUS.US_BLOCKING:
                UpdateBlocking();
                break;
            case UNIT_STATUS.US_DYING:
                UpdateDying();
                break;
            case UNIT_STATUS.US_KNOCKBACK:
                UpdateKnockback();
                break;
        }
    }
    //----------------------------------------
    protected override void MoveUnit()
    {
        float speed = Time.deltaTime * mMovingSpeed;

        Vector3 moveDelta = mMoveDirection * speed;

        Vector3 oldPos = GetPos();

        Vector3 pos = oldPos;

        pos += moveDelta;

        if (mMapManager.CheckInside(pos))
            Move(moveDelta.x, moveDelta.y);
    }
    //----------------------------------------
    private void ProcessKey()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (DoAttack())
                return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3Int pos = CGameManager.GetInstance().GetTilemap().WorldToCell(GetPos());
            TileBase tileBase = CGameManager.GetInstance().GetTilemap().GetTile(pos);

            Debug.Log(string.Format("tile map pos {0}, {1}, {2} => {3}", pos.x, pos.y, pos.z, tileBase != null ? tileBase.name : "notile"));
        }

        if (Input.GetKeyDown(KeyCode.A))
            mLeftOn = true;
        if (Input.GetKeyUp(KeyCode.A))
            mLeftOn = false;

        if (Input.GetKeyDown(KeyCode.D))
            mRightOn = true;
        if (Input.GetKeyUp(KeyCode.D))
            mRightOn = false;

        if (Input.GetKeyDown(KeyCode.W))
            mUpOn = true;
        if (Input.GetKeyUp(KeyCode.W))
            mUpOn = false;

        if (Input.GetKeyDown(KeyCode.S))
            mDownOn = true;
        if (Input.GetKeyUp(KeyCode.S))
            mDownOn = false;

        bool moving = mLeftOn || mRightOn || mUpOn || mDownOn;

        if (!moving)
        {
            mMoveMask = 0;

            if (IsMoving())
            {
                SetStatus(UNIT_STATUS.US_IDLE);
            }

            return;
        }

        int lastMask = mMoveMask;
        mMoveMask = 0;

        mMoveDirection = Vector3.zero;

        if (mLeftOn)
        {
            mRightOn = false;
            mMoveDirection += Vector3.left;

            mMoveMask |= 0x00000001;
        }
        else if (mRightOn)
        {
            mMoveDirection += Vector3.right;
            mMoveMask |= 0x00000002;
        }

        if (mUpOn)
        {
            mDownOn = false;
            mMoveDirection += Vector3.up;
            mMoveMask |= 0x00000004;
        }
        else if (mDownOn)
        {
            mMoveDirection += Vector3.down;
            mMoveMask |= 0x00000008;
        }

        mMoveDirection.Normalize();

        if (lastMask != mMoveMask)
        {
            float angle = CAngleUtil.GetAngle(mMoveDirection);

            if ((angle >= 315 && angle < 360) || (angle >= 0 && angle <= 45))
                PlayAnimation("cavemanup");
            else if (angle >= 45 && angle <= 135)
                PlayAnimation("cavemanright");
            else if (angle >= 135 && angle <= 225)
                PlayAnimation("cavemandown");
            else if (angle >= 225 && angle <= 315)
                PlayAnimation("cavemanleft");
        }

        if (CheckMoveable())
        {
            SetStatus(UNIT_STATUS.US_MOVE);
        }
        //Debug.Log(string.Format("angle : {0}", CAngleUtil.GetAngle(mMoveDirection)));
    }
    //----------------------------------------
    public override bool CheckAttackable()
    {
        if (mCurrentAttackSpeed > 0.0f)
            return false;

        return base.CheckAttackable();
    }

    bool DoAttack()
    {
        //공격가능한 상태가 아닌가?
        if (!CheckAttackable())
            return false;

        mCurrentMovingDelay = mMovingDelay;

        Vector3 pos = GetPos();

        pos += mMoveDirection * 10;

        Vector3 centerPos = CGameManager.GetInstance().GetPos();

        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        mousePos.x -= Screen.width / 2;
        mousePos.y -= Screen.height / 2;

        mousePos += centerPos;

        Debug.Log(string.Format("mouse {0}, {1}", mousePos.x, mousePos.y));
        CGameManager.GetInstance().CreatePlayerBullet(pos.x, pos.y, mousePos.x, mousePos.y);

        SetStatus(UNIT_STATUS.US_ATTACK);
        mAttackDelay = mAttackTime;
        mCurrentAttackSpeed = mAttackSpeed;

        return true;
    }

    void UpdateAttack()
    {
        mAttackDelay -= Time.deltaTime;

        if (mAttackDelay <= 0)
        {
            SetStatus(UNIT_STATUS.US_IDLE);
            return;
        }
    }

    //----------------------------------------
    void SetBlocking()
    {
        mCurrentBlockingTime = mMaxBlockingTime;
        SetStatus(UNIT_STATUS.US_BLOCKING);
    }

    void UpdateBlocking()
    {
        mCurrentBlockingTime -= Time.deltaTime;

        if (mCurrentBlockingTime <= 0)
        {
            SetStatus(UNIT_STATUS.US_IDLE);
            return;
        }
    }

    public override void SetDamage(int damage)
    {
        float blockRate = mBlockingRate;

        if (GetStatus() != UNIT_STATUS.US_IDLE)
            blockRate *= 0.7f;

        if ((Random.value * 100.0f) <= blockRate)
        {
#if UNITY_EDITOR
            Debug.Log("blocking");
#endif
            switch (GetStatus())
            {
                case UNIT_STATUS.US_DYING:
                    return;
            }

            SetBlocking();
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("damage : " + damage);
#endif
            base.SetDamage(damage);

            if (GetHp() <= 0)
            {
                SetDying();
            }
        }
    }
    //----------------------------------------
    public void SetDying()
    {
        SetStatus(UNIT_STATUS.US_DYING);
        SetAlpha(0.0f);
        EnableCollider(false);
        CGameManager.GetInstance().CreateEffect(GetPos().x, GetPos().y);

        CGameManager.GetInstance().OnUnitDying(this);
    }

    void UpdateDying()
    {
        mDyingTime -= Time.deltaTime;

        if (mDyingTime <= 0)
        {
            SetStatus(UNIT_STATUS.US_DIED);
            Destroy(gameObject, 0.5f);
        }
    }

    private void OnDestroy()
    {
        if (CGameManager.GetInstance() != null)
            CGameManager.GetInstance().OnUnitDestroyed(this);
    }
    //----------------------------------------
    public override void SetKnockback(Vector3 forceOrigin, float force, float knockbackTime)
    {
        if (GetStatus() == UNIT_STATUS.US_DYING)
            return;

        SetStatus(UNIT_STATUS.US_KNOCKBACK);

        mKnockbackForce = force;
        mKnockbackTime = 0.0f;
        mKnockbackMaxTime = knockbackTime;

        mKnockbackDirection = GetPos() - forceOrigin;
        mKnockbackDirection.Normalize();
    }

    void UpdateKnockback()
    {
        float force = mKnockbackForce * (Time.deltaTime / mKnockbackMaxTime);

        Vector3 oldPos = GetPos();

        Vector3 delta = mKnockbackDirection * force;

        delta += oldPos;

        if (mMapManager.CheckInside(delta))
        {
            SetPos(delta.x, delta.y);

            mKnockbackTime += Time.deltaTime;

            if (mKnockbackTime >= mKnockbackMaxTime)
            {
                SetStatus(UNIT_STATUS.US_IDLE);
            }
        }
        else
            SetStatus(UNIT_STATUS.US_IDLE);
    }
}
