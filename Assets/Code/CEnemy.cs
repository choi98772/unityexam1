using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnemy : CUnit
{
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
    public float mSight = 1000.0f;

    private float mAIDelay = 0.0f;

    private Vector3 mTargetPos;
    private float mSpeedScale = 1.0f;
    private CMapManager mMapManager = null;

    public override void Initialize()
    {
        base.Initialize();
        SetUnitType(UNIT_TYPE.ENEMY);

        CGameManager.GetInstance().OnEnemyAwake(this);

        mMapManager = CGameManager.GetInstance().GetMapManager();
    }

    public override void UpdateUnit()
    {
        if (mCurrentAttackSpeed > 0.0f)
        {
            mCurrentAttackSpeed -= Time.deltaTime;
        }

        ProcessAI();

        switch (GetStatus())
        {
            case UNIT_STATUS.US_IDLE:
                break;
            case UNIT_STATUS.US_MOVE_TARGET:
                MoveTarget();
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
    private void ProcessAI()
    {
        if (mAIDelay > 0.0f)
        {
            mAIDelay -= Time.deltaTime;
            return;
        }

        mAIDelay = 0.1f;

        if (!CheckMoveable())
            return;

        if (CGameManager.GetInstance().FindPlayerBulletInRange(GetPos(), 200.0f))
        {
            SetMoveRandomTargetPos();
            return;
        }

        CPlayer player = CGameManager.GetInstance().GetPlayer();

        if (player == null || !player.IsLive())
        {
            SetStatus(UNIT_STATUS.US_IDLE);
            return;
        }

        Vector3 targetPos = player.GetPos();

        mMoveDirection = targetPos - GetPos();
        mMoveDirection.Normalize();

        float playerRange = GetDistanceSq(player);

        if (playerRange <= (mSight * mSight))
        {
            mTargetPos = targetPos;

            UpdateUnitAngle();

            //플레이어가 공격가능 거리에 있다면 공격하기
            DoAttack(targetPos);
        }
        else
        {
            //공격가능거리 밖이면 추적하기
            mTargetPos = targetPos;

            UpdateUnitAngle();

            SetStatus(UNIT_STATUS.US_MOVE_TARGET);
        }
    }

    private void UpdateUnitAngle()
    {
        int moveMask = 0;

        float angle = CAngleUtil.GetAngle(mMoveDirection);

        if ((angle >= 315 && angle < 360) || (angle >= 0 && angle <= 45))
        {
            moveMask = 1;
        }
        else if (angle >= 45 && angle <= 135)
        {
            moveMask = 2;
        }
        else if (angle >= 135 && angle <= 225)
        {
            moveMask = 3;
        }
        else if (angle >= 225 && angle <= 315)
        {
            moveMask = 4;
        }

        if (moveMask != mMoveMask)
        {
            mMoveMask = moveMask;

            switch (moveMask)
            {
                case 1:
                    PlayAnimation("cavemanup");
                    break;
                case 2:
                    PlayAnimation("cavemanright");
                    break;
                case 3:
                    PlayAnimation("cavemandown");
                    break;
                case 4:
                    PlayAnimation("cavemanleft");
                    break;
            }
        }
    }
    //----------------------------------------
    private void SetMoveRandomTargetPos()
    {
        if (GetStatus() == UNIT_STATUS.US_MOVE_TARGET)
            return;

        Vector3 unitPos = GetPos();
        Vector3 pos = unitPos;

        pos.x += ((2.0f *Random.value) - 1.0f) * 500.0f;
        pos.y += ((2.0f * Random.value) - 1.0f) * 500.0f;

        CGameManager.GetInstance().GetMapManager().ValidatePos(ref pos);

        mTargetPos = pos;

        mMoveDirection = mTargetPos - unitPos;
        mMoveDirection.Normalize();
        mSpeedScale = 1.5f;
        SetStatus(UNIT_STATUS.US_MOVE_TARGET);
    }

    private void MoveTarget()
    {
        float speed = Time.deltaTime * mMovingSpeed * mSpeedScale;

        Vector3 delta = mTargetPos - GetPos();

        if (delta.sqrMagnitude < (speed * speed))
        {
            mSpeedScale = 1.0f;
            SetPos(mTargetPos.x, mTargetPos.y);
            SetStatus(UNIT_STATUS.US_IDLE);
            return;
        }

        delta = mMoveDirection * speed;

        Move(delta.x, delta.y);
    }
    //----------------------------------------
    public override bool CheckAttackable()
    {
        if (mCurrentAttackSpeed > 0.0f)
            return false;

        return base.CheckAttackable();
    }

    private bool DoAttack(Vector3 targetPos)
    {
        //공격가능한 상태가 아닌가?
        if (!CheckAttackable())
            return false;

        mCurrentMovingDelay = mMovingDelay;

        Vector3 pos = GetPos();

        pos += mMoveDirection * 10;

        
        CGameManager.GetInstance().CreateEnemyBullet(pos.x, pos.y, targetPos.x, targetPos.y);

        SetStatus(UNIT_STATUS.US_ATTACK);
        mAttackDelay = mAttackTime;
        mCurrentAttackSpeed = mAttackSpeed;
        return true;
    }

    private void UpdateAttack()
    {
        mAttackDelay -= Time.deltaTime;

        if (mAttackDelay <= 0)
        {
            SetStatus(UNIT_STATUS.US_IDLE);
            return;
        }
    }

    //----------------------------------------
    private void SetBlocking()
    {
        mCurrentBlockingTime = mMaxBlockingTime;
        SetStatus(UNIT_STATUS.US_BLOCKING);
    }

    private void UpdateBlocking()
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

    private void UpdateDying()
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

    private void UpdateKnockback()
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
