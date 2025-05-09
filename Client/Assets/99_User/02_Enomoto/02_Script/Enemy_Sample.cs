using UnityEngine;

public class Enemy_Sample : Enemy_abs
{
    public enum ANIM_ID
    {
        Idle = 1,
        Attack,
        Run,
        Hit,
    }

    #region ステータス管理
    [Header("ステータス設定")]
    [SerializeField] int _hp;
    [SerializeField] int _power;
    [SerializeField] int _speed;
    [SerializeField] float _viewRadius;
    [SerializeField] float _attackRange;
    #endregion

    #region コンポーネント管理
    [Header("コンポーネント")]
    [SerializeField] Animator _animatorSelf;
    Rigidbody2D m_rb2;
    #endregion

    // マネージャークラスから取得できるのが理想
    [SerializeField] GameObject target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AnimatorSelf = _animatorSelf;
        HP = _hp; 
        Power = _power;
        Speed = _speed;
        ViewRadius = _viewRadius;
        AttackRange = _attackRange;

        m_rb2 = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        // 進行方向にテクスチャを反転
        if (m_rb2.linearVelocityX > 0 && transform.localScale.x < 0
|| m_rb2.linearVelocityX < 0 && transform.localScale.x > 0) Flip();

        // ターゲットが視野に入っているかどうか
        float disToTarget = Vector3.Distance(this.transform.position, target.transform.position);
        if (disToTarget <= _viewRadius)
        {
            if (disToTarget <= _attackRange)
            {
                Attack();
            }
            else
            {
                Run();
            }
        }
        else
        {
            Idle();
        }

    }

    public override void Idle()
    {
        SetAnimId((int)ANIM_ID.Idle);
        m_rb2.linearVelocity = Vector2.zero;
    }

    public override void Attack()
    {
        SetAnimId((int)ANIM_ID.Attack);
        m_rb2.linearVelocity = Vector2.zero;
    }

    public override void Run()
    {
        SetAnimId((int)ANIM_ID.Run);
        float distToPlayer = target.transform.position.x - this.transform.position.x;
        m_rb2.linearVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * _speed, m_rb2.linearVelocity.y);
    }

    public override void ApplyDamage(int damage)
    {
        SetAnimId((int)ANIM_ID.Hit);
    }
}