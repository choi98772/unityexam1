using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUnit : MonoBehaviour {
    public enum UNIT_STATUS
    {
        US_IDLE = 0, //대기중
        US_MOVE, //이동중
        US_MOVE_TARGET, //목표지점으로 이동중
        US_ATTACK, //공격중
        US_BLOCKING, //공격 방어중
        US_TRACE,   //적을 추적중
        US_SEARCH, //적을 탐색중
        US_KNOCKBACK, //넉백됨
        US_DYING,  //사망
        US_DIED,    //사망함
    }

    public float mMovingDelay = 0.7f;

    protected UNIT_TYPE mUnitType; //유닛타입
    protected Vector3 mMoveDirection = Vector3.down; //이동방향
    public float mMovingSpeed = 10.0f; //이동속도
    protected int mHp = 0; //현재 hp
    public int mMaxHp = 0; //최대 hp

    protected Transform mTransform = null;
    protected Animator mAnimator = null;
    protected float mCurrentMovingDelay = 0.0f;
    protected UNIT_STATUS mStatus = UNIT_STATUS.US_IDLE;
    protected SpriteRenderer mRenderer = null;
    protected Collider2D mCollider = null;
    protected int mUnitId = 0;
    //--------------------------------------------------------------------
    public UNIT_STATUS GetStatus() { return mStatus; }

    public void SetStatus(UNIT_STATUS status) { mStatus = status; }
    //--------------------------------------------------------------------
    public void SetUnitId(int id) { mUnitId = id; }
    public int GetUnitId() { return mUnitId; }
    //--------------------------------------------------------------------
    //유닛이 이동 가능한 상태인가?
    public bool CheckMoveable()
    {
        switch(GetStatus())
        {
            case UNIT_STATUS.US_IDLE:
            case UNIT_STATUS.US_MOVE_TARGET:
            case UNIT_STATUS.US_SEARCH:
            case UNIT_STATUS.US_MOVE:
            case UNIT_STATUS.US_TRACE:
                return true;
        }

        return false;
    }

    //유닛이 공격 가능 상태에 있는가?
    public virtual bool CheckAttackable()
    {
        switch (GetStatus())
        {
            case UNIT_STATUS.US_IDLE:
            case UNIT_STATUS.US_MOVE_TARGET:
            case UNIT_STATUS.US_SEARCH:
            case UNIT_STATUS.US_MOVE:
            case UNIT_STATUS.US_TRACE:
                return true;
        }

        return false;
    }

    public bool IsLive()
    {
        switch(GetStatus())
        {
            case UNIT_STATUS.US_DYING:
            case UNIT_STATUS.US_DIED:
                return false;
        }

        return true;
    }

    public bool IsMoving()
    {
        return GetStatus() == UNIT_STATUS.US_MOVE;
    }
    //--------------------------------------------------------------------
    //유닛 타입얻기
    public UNIT_TYPE GetUnitType()
    {
        return mUnitType;
    }

    //유닛타입설정
    public void SetUnitType(UNIT_TYPE type)
    {
        mUnitType = type;
    }

    //---------------------------------------------
    public int GetHp() { return mHp; } //현재 hp얻기
    public int GetMaxHp() { return mMaxHp; }

    //hp 설정
    public void SetHp(int hp, int maxHp)
    {
        mHp = hp;
        mMaxHp = maxHp;
    }

    public void SetHpOnly(int hp)
    {
        mHp = hp;
    }

    public void SetMaxHp(int maxHp)
    {
        mMaxHp = maxHp;
    }

    public virtual void SetDamage(int damage)
    {
        mHp -= damage;
    }

    public float GetHpPercent()
    {
        if (mMaxHp < 1)
            return 0;

        return (float)mHp / (float)mMaxHp;
    }
    //---------------------------------------------
    //위치 설정
    public void SetPos(float x, float y)
    {
        if (mTransform == null)
            mTransform = gameObject.transform;

        mTransform.localPosition = new Vector3(x, y, 0);
    }

    //위치 얻기
    public Vector3 GetPos()
    {
        return mTransform.localPosition;
    }

    //dx, dy만큼 이동
    public void Move(float dx, float dy)
    {
        Vector3 pos = GetPos();

        pos.x += dx;
        pos.y += dy;

        mTransform.localPosition = pos;
    }
    //---------------------------------------------
    // Use this for initialization
    void Start () {
        mAnimator = gameObject.GetComponent<Animator>();
        mTransform = gameObject.transform;
        Initialize();
	}
	
    //유닛초기화 수행
    public virtual void Initialize()
    {
        mHp = mMaxHp;
        mCollider = gameObject.GetComponent<Collider2D>();
        mRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    //유닛 갱신 수행
    public virtual void UpdateUnit()
    {

    }

    //애니메이션 플레이
    public void PlayAnimation(string name)
    {
        if (mAnimator != null)
            mAnimator.Play(name);
    }

    //유닛 이동중인경우 이동처리
    protected virtual void MoveUnit()
    {
        float speed = Time.deltaTime * mMovingSpeed;

        Vector3 moveDelta = mMoveDirection * speed;

        Move(moveDelta.x, moveDelta.y);
    }

    protected void SetMoveDirection(float x, float y, float tx, float ty)
    {
        Vector3 v = new Vector3(tx - x, ty - y, 0.0f);

        v.Normalize();

        mMoveDirection = v;
    }

    //충돌발생시 호출됨
    protected virtual void OnTriggerEntered(CUnit target)
    {
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        CUnit unit = collider.gameObject.GetComponent<CUnit>();

        if (unit != null)
            OnTriggerEntered(unit);
    }

    //unit과 현재 유닛간의 거리의 제곱을 얻기
    public float GetDistanceSq(CUnit unit)
    {
        Vector3 v = unit.GetPos() - GetPos();

        return (v.x * v.x) + (v.y * v.y);
    }

    public float GetDistanceSq(Vector3 pos)
    {
        Vector3 v = pos - GetPos();

        return (v.x * v.x) + (v.y * v.y);
    }

    public virtual void SetKnockback(Vector3 forceOrigin, float force, float knockbackTime)
    {
    }

    public void SetAlpha(float alpha)
    {
        if (mRenderer == null)
            return;

        Color color = mRenderer.color;
        color.a = alpha;
        mRenderer.color = color;
    }

    public void EnableCollider(bool isEnabled)
    {
        mCollider.enabled = isEnabled;
    }

    public virtual void OnUnitDying(CUnit unit)
    {

    }
}
