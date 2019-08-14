using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBullet : CUnit {

    public int mMinDamage = 0; //데미지
    public int mMaxDamage = 0; //데미지
    public float mSight = 200.0f; //추적가능거리
    public float mLifeTime = 10.0f; //이시간동안만 존재할 수 있음
    public float mKnockbackRate = 30.0f;
    public float mKnockbackForce = 100.0f;
    public float mKnockbackTime = 0.2f;

    float mDistanceLimit = 5000.0f;
    float mDamageSkipTime = 0.0f; //여기에 설정된 시간동안은 데미지를 주지 않음
    int mHitCount = 0; //적을 타격시마다 1증가함
    CUnit mTarget = null; //추적대상
    int mLastDirection = 0;
    Vector3 mTargetPos;
    float mTraceDelayTime = 0.0f;
    int mSlotIndex = -1;
    float mPenetrationRate = 70.0f;
    

    public override void Initialize()
    {
        UpdateDirectionAni();
        base.Initialize();
    }

    public override void UpdateUnit()
    {
        mLifeTime -= Time.deltaTime;

        if(mLifeTime <= 0.0f || mDistanceLimit <= 0.0f)
        {
            DestroyBullet();
            return;
        }

        switch (GetStatus())
        {
            case UNIT_STATUS.US_MOVE_TARGET: //목표점까지 이동해야함
                MoveToTarget();
                break;
            case UNIT_STATUS.US_MOVE: //현재 방향으로 이동중
                MoveBullet();
                break;
            case UNIT_STATUS.US_SEARCH:
                SearchTarget();
                break;
            case UNIT_STATUS.US_TRACE: //현재 방향으로 이동하면서 대상을 추적함
                TraceTarget();
                break;
        }
    }

    void DestroyBullet()
    {
        Debug.Log("remove bullet");
        Destroy(gameObject);
        CGameManager.GetInstance().RemoveBullet(mSlotIndex, GetUnitType());
    }

    protected override void MoveUnit()
    {
        float speed = Time.deltaTime * mMovingSpeed;

        mDistanceLimit -= speed;

        Vector3 moveDelta = mMoveDirection * speed;

        Move(moveDelta.x, moveDelta.y);
    }

    void MoveBullet()
    {
        if (mDamageSkipTime > 0.0f)
        {
            mDamageSkipTime -= Time.deltaTime;

            if (mDamageSkipTime <= 0.0f)
            {
                SetStatus(UNIT_STATUS.US_SEARCH);
                EnableCollider(true);
            }
        }

        MoveUnit();
    }

    //목표점까지 이동하기
    void MoveToTarget()
    {
        float speed = Time.deltaTime * mMovingSpeed;
        mDistanceLimit -= speed;

        Vector3 distance = GetPos() - mTargetPos;

        //목표지점에 거의 도달했다면
        if (distance.sqrMagnitude <= (speed * speed))
        {
            SetStatus(UNIT_STATUS.US_SEARCH);
        }

        Vector3 moveDelta = mMoveDirection * speed;

        Move(moveDelta.x, moveDelta.y);
    }

    //대상 추적하기
    void SearchTarget()
    {
        MoveUnit();

        //추적할 대상이 없음
        CUnit enemy = CGameManager.GetInstance().GetEnemy(this);

        if (enemy == null || !enemy.IsLive())
        {
            if (mTarget == null)
                SetStatus(UNIT_STATUS.US_MOVE);
            else
            {
                if (mTarget.GetDistanceSq(this) > (mSight * mSight))
                {
                    mTarget = null;
                    SetStatus(UNIT_STATUS.US_MOVE);
                }
                else
                    SetStatus(UNIT_STATUS.US_TRACE);
            }

            return;
        }

        //대상이 있으면 추적으로 설정하기
        mTarget = enemy;

        if (mTarget.GetDistanceSq(this) > (mSight * mSight))
        {
            mTarget = null;
            SetStatus(UNIT_STATUS.US_MOVE);
        }
        else
        {
            mTargetPos = mTarget.GetPos();

            mMoveDirection = mTargetPos - GetPos();
            mMoveDirection.Normalize();

            SetStatus(UNIT_STATUS.US_MOVE_TARGET);
        }
    }

    void TraceTarget()
    {
        Vector3 pos = GetPos();
        Vector3 targetPos = mTarget.GetPos();

        SetMoveDirection(pos.x, pos.y, targetPos.x, targetPos.y);
        UpdateDirectionAni();
        MoveUnit();        
    }

    public void Init(Transform parentTransform, UNIT_TYPE type, int slotIndex, float x, float y, float tx, float ty)
    {
        mTransform = gameObject.transform;
        gameObject.transform.parent = parentTransform;
        SetStatus(UNIT_STATUS.US_MOVE_TARGET);
        mSlotIndex = slotIndex;

        SetUnitType(type);

        mTargetPos = new Vector3(tx, ty, 0);
        SetPos(x, y);
        SetMoveDirection(x, y, tx, ty);
    }

    string GetAnimationName(int dir)
    {
        if (GetUnitType() == UNIT_TYPE.BULLET_PLAYER)
        {
            switch(dir)
            {
                case 1:
                    return "pbup";
                case 2:
                    return "pbright";
                case 3:
                    return "pbdown";
                case 4:
                    return "pbleft";
            }
        }
        else
        {
            switch (dir)
            {
                case 1:
                    return "ebup";
                case 2:
                    return "ebright";
                case 3:
                    return "ebdown";
                case 4:
                    return "ebleft";
            }
        }

        return string.Empty;
    }

    protected void UpdateDirectionAni()
    {
        /*float angle = CAngleUtil.GetAngle(mMoveDirection);

        if ((angle >= 315 && angle < 360) || (angle >= 0 && angle <= 45))
        {
            if (mLastDirection != 1)
            {
                mLastDirection = 1;
                PlayAnimation(GetAnimationName(mLastDirection));
            }
        }
        else if (angle >= 45 && angle <= 135)
        {
            if (mLastDirection != 2)
            {
                mLastDirection = 2;
                PlayAnimation(GetAnimationName(mLastDirection));
            }
        }
        else if (angle >= 135 && angle <= 225)
        {
            if (mLastDirection != 3)
            {
                mLastDirection = 3;
                PlayAnimation(GetAnimationName(mLastDirection));
            }
        }
        else if (angle >= 225 && angle <= 315)
        {
            if (mLastDirection != 4)
            {
                mLastDirection = 4;
                PlayAnimation(GetAnimationName(mLastDirection));
            }
        }*/
    }

    protected override void OnTriggerEntered(CUnit target)
    {
        //대상이 없거나, 데미지 무시 기간중이면
        if (target == null || mDamageSkipTime > 0.0f)
            return;

        switch(target.GetUnitType())
        {
            case UNIT_TYPE.PLAYER:
                {
                    if (GetUnitType() != UNIT_TYPE.BULLET_ENEMY)
                        return;
                }
                break;
            case UNIT_TYPE.ENEMY:
                {
                    if (GetUnitType() != UNIT_TYPE.BULLET_PLAYER)
                        return;
                }
                break;

            default:
                return;
        }

        //대상에게 데미지 주기
        target.SetDamage(GetDamage());

        if ((Random.value * 100.0f) <= mKnockbackRate)
        {
            target.SetKnockback(GetPos(), mKnockbackForce, mKnockbackTime);
        }


        mDamageSkipTime = 0.3f;
        EnableCollider(false);
        SetStatus(UNIT_STATUS.US_MOVE);

        Vector3 pos = GetPos();

        CGameManager.GetInstance().CreateEffect(pos.x, pos.y);

        if (mPenetrationRate < (Random.value * 100.0f))
        {
            DestroyBullet();
            return;
        }

        mPenetrationRate -= 40.0f;
    }

    public int GetDamage()
    {
        return Random.Range(mMinDamage, mMaxDamage + 1);
    }

    public override void OnUnitDying(CUnit unit)
    {
        if (mTarget != null && mTarget.GetUnitId() == unit.GetUnitId())
        {
            mTarget = null;
            mDamageSkipTime = 0.0f;
            SetStatus(UNIT_STATUS.US_MOVE);
        }
    }
}
