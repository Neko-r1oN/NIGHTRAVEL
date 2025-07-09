//**************************************************
//  [ボス] フルメタルワームの体の管理クラス
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullMetalBody : EnemyBase
{
    /// <summary>
    /// 役割の種別
    /// </summary>
    public enum ROLE_TYPE
    {
        None,
        Spawner,    // ザコ敵生成
        Attacker    // 攻撃
    }

    [SerializeField] 
    ROLE_TYPE roleType;
    public ROLE_TYPE RoleType { get { return roleType; } }

    FullMetalWorm worm;

    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        None = 0,
        Open,
        Close,
        Dead,
    }

    #region 攻撃関連
    [Foldout("攻撃関連")]
    [SerializeField]
    List<Transform> aimTransformList;
    [Foldout("攻撃関連")]
    [SerializeField]
    List<GunParticleController> gunPsControllerList;
    [Foldout("攻撃関連")]
    [SerializeField]
    float gunBulletWidth = 0;
    [Foldout("攻撃関連")]
    [SerializeField]
    float aimRotetionSpeed = 3f;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = false;
        isSpawn = false;
        worm = transform.parent.GetComponent<FullMetalWorm>();
    }

    private void OnValidate()
    {
        switch (roleType)
        {
            case ROLE_TYPE.None:
                break;
            case ROLE_TYPE.Spawner:
                break;
            case ROLE_TYPE.Attacker:
                break;
        }
    }

    /// <summary>
    /// 行動パターン実行処理
    /// </summary>
    protected override void DecideBehavior(){}

    /// <summary>
    /// ロールに応じた行動を実行するコルーチン
    /// </summary>
    /// <returns></returns>
    public IEnumerator ActByRoleTypeCoroutine()
    {
        yield return null;
    }

    #region 攻撃処理関連

    /// <summary>
    /// 攻撃処理
    /// </summary>
    public void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        cancellCoroutines.Add(StartCoroutine(RangeAttack()));
    }

    /// <summary>
    /// 遠距離攻撃開始
    /// </summary>
    /// <returns></returns>
    IEnumerator RangeAttack()
    {
        gunPsControllerList.ForEach(item => { item.StartShooting(); });

        float time = 0;
        float waitSec = 0.05f;
        while (time < shotsPerSecond)
        {
            // ターゲットが追跡範囲外 || ターゲットが存在しない場合
            if (target && disToTarget > trackingRange || !target) SetNearTarget();

            // 追跡範囲内のターゲットのいる方向に向かってエイム
            if (target && disToTarget <= trackingRange)
            {
                foreach (var aimTransform in aimTransformList)
                {
                    Vector3 direction = (target.transform.position - aimTransform.position).normalized;
                    Quaternion quaternion = Quaternion.Euler(0, 0, aimTransform.GetComponent<EnemyProjectileChecker>().ClampAngleToTarget(direction));
                    aimTransform.rotation = Quaternion.RotateTowards(aimTransform.rotation, quaternion, aimRotetionSpeed);
                }
            }
            yield return new WaitForSeconds(waitSec);
            time += 0.1f;
        }

        gunPsControllerList.ForEach(item => { item.StopShooting(); });

        // 銃を元の角度に戻す
        while (aimTransformList.Find(item => item.localEulerAngles.z != 0))
        {
            foreach (var aimTransform in aimTransformList)
            {
                Quaternion resetQuaternion = Quaternion.Euler(0, 0, 0);
                aimTransform.rotation = Quaternion.RotateTowards(aimTransform.rotation, resetQuaternion, aimRotetionSpeed * 2);
            }
            yield return new WaitForSeconds(waitSec);
        }
    }

    #endregion

    #region 敵の生成関連

    /// <summary>
    /// ザコ敵を複数生成するコルーチンの実行
    /// </summary>
    public bool RunEnemySpawn()
    {
        isAttacking = true;
        bool isGenerateSucsess = false;
        int generatedEnemiesCnt = worm.GeneratedEnemies.Count;

        GameObject nearPlayer = GetNearPlayer();
        if (generatedEnemiesCnt >= worm.GeneratedMax || nearPlayer == null) return false;   // これ以上敵の生成ができない || 近くにプレイヤーが存在しない

        float distToNearPlayer = Vector2.Distance(transform.position, nearPlayer.transform.position);
        bool isPlayerNearby = distToNearPlayer <= worm.DistToPlayerMin;

        // 生成位置の近くにプレイヤーがいる && 生成位置がステージの範囲内 && 生成位置が壁に埋まっていない場合
        if (isPlayerNearby
            && TransformUtils.IsWithinBounds(transform, worm.MinRange, worm.MaxRange)
            && !Physics2D.OverlapCircle(transform.position, worm.TerrainCheckRane, terrainLayerMask))
        {
            isGenerateSucsess = true;
            int maxEnemies = Random.Range(1, 4);
            if (generatedEnemiesCnt + maxEnemies > worm.GeneratedMax) maxEnemies = worm.GeneratedMax - generatedEnemiesCnt;

            cancellCoroutines.Add(StartCoroutine(GenerateEnemeiesCoroutine(maxEnemies)));
        }

        return isGenerateSucsess;
    }

    /// <summary>
    /// ザコ敵を複数生成するコルーチン
    /// </summary>
    IEnumerator GenerateEnemeiesCoroutine(int maxEnemies)
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            Random.InitState(System.DateTime.Now.Millisecond);  // 乱数のシード値を更新
            float time = Random.Range(0f, 0.5f);
            yield return new WaitForSeconds(time);
            if (worm.GeneratedEnemies.Count >= worm.GeneratedMax) yield break;  // 既に生成上限に達している場合

            // ここに生成する処理 && ハッチが開くアニメーション####################################
            Random.InitState(System.DateTime.Now.Millisecond);  // 乱数のシード値を更新
            worm.GeneratedEnemies.Add(GenerateEnemy(transform.position));
        }
    }

    /// <summary>
    /// ザコ敵を生成する処理
    /// </summary>
    GameObject GenerateEnemy(Vector2 point)
    {
        var enemyObj = Instantiate(worm.EnemyPrefabs[(int)Random.Range(0, worm.EnemyPrefabs.Count)], point, Quaternion.identity).gameObject;
        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();

        if ((int)Random.Range(0, 2) == 0) enemy.Flip();    // 確率で向きが変わる
        enemy.TransparentSprites();
        enemy.Players = GetAlivePlayers();
        return enemyObj;
    }

    #endregion

    #region ヒット処理関連

    /// <summary>
    /// 死亡するときに呼ばれる処理処理
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        SetAnimId((int)ANIM_ID.Dead);
    }

    #endregion

    #region チェック処理関連

    /// <summary>
    /// 検出範囲の描画処理
    /// </summary>
    protected override void DrawDetectionGizmos()
    {
        // 攻撃開始距離
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackDist);

        // 攻撃範囲
        //if (meleeAttackCheck)
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawWireSphere(meleeAttackCheck.transform.position, meleeAttackRange);
        //}

        // 射線
        SetNearTarget();
        foreach (var aimTransform in aimTransformList)
        {
            aimTransform.GetComponent<EnemyProjectileChecker>().DrawProjectileRayGizmo(gunBulletWidth, true);
        }
    }

    #endregion
}
