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
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        None = 0,
        Open,
        Close,
        Dead,
    }

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

    #region コンポーネント関連
    FullMetalWorm worm;
    #endregion

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
        isInvincible = false;
        worm = transform.parent.GetComponent<FullMetalWorm>();
    }

    /// <summary>
    /// 行動パターン実行処理
    /// </summary>
    protected override void DecideBehavior(){}

    /// <summary>
    /// ロールに応じた行動を実行するコルーチン
    /// </summary>
    /// <returns></returns>
    public void ActByRoleType()
    {
        switch (roleType)
        {
            case ROLE_TYPE.None:
                break;
            case ROLE_TYPE.Spawner:
                RunEnemySpawn();
                break;
            case ROLE_TYPE.Attacker:
                Attack();
                break;
            default:
                break;
        }
    }

    #region 攻撃処理関連

    /// <summary>
    /// 攻撃処理
    /// </summary>
    void Attack()
    {
        cancellCoroutines.Add(StartCoroutine(RangeAttack()));
    }

    /// <summary>
    /// 遠距離攻撃開始
    /// </summary>
    /// <returns></returns>
    IEnumerator RangeAttack()
    {
        gunPsControllerList.ForEach(item => { item.StartShooting(); });

        Dictionary<Transform, GameObject> targetList = new Dictionary<Transform, GameObject>();
        float time = 0;
        float waitSec = 0.05f;
        while (time < shotsPerSecond)
        {
            foreach (var aimTransform in aimTransformList)
            {
                EnemyProjectileChecker projectileChecker = aimTransform.GetComponent<EnemyProjectileChecker>();

                // 初回時にのみ処理
                if (!targetList.ContainsKey(aimTransform)) 
                    targetList.Add(aimTransform, projectileChecker.GetNearPlayerInSight(Players, true));

                // ターゲットを見失ったら、ターゲットを再設定
                if (targetList[aimTransform] == null || !projectileChecker.IsTargetInSight(targetList[aimTransform]))
                    targetList[aimTransform] = projectileChecker.GetNearPlayerInSight(Players, true);

                if (targetList[aimTransform] != null)
                {
                    Vector3 direction = (targetList[aimTransform].transform.position - aimTransform.position).normalized;
                    projectileChecker.RotateAimTransform(direction, aimRotetionSpeed);
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
                aimTransform.localRotation = Quaternion.RotateTowards(aimTransform.localRotation, resetQuaternion, aimRotetionSpeed * 2);
            }
            yield return new WaitForSeconds(waitSec);
        }
    }

    #endregion

    #region 敵の生成関連

    /// <summary>
    /// ザコ敵を複数生成するコルーチンの実行
    /// </summary>
    bool RunEnemySpawn()
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
            int maxEnemies = Random.Range(1, 3);
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
            Random.InitState(System.DateTime.Now.Millisecond + i);  // 乱数のシード値を更新
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
        StopAllCoroutines();
        gunPsControllerList.ForEach(item => { item.StopShooting(); });
    }

    /// <summary>
    /// ダメージ適用処理
    /// </summary>
    /// <param name="power"></param>
    /// <param name="attacker"></param>
    /// <param name="effectTypes"></param>
    public override void ApplyDamage(int power, Transform attacker = null, bool drawDmgText = true, params StatusEffectController.EFFECT_TYPE[] effectTypes)
    {
        attacker = null;
        base.ApplyDamage(power, attacker, true, effectTypes);

        // 本体のHPも削る
        worm.ApplyDamage(power, attacker, false);

        // 被ダメージ量を表示する
        var damage = CalculationLibrary.CalcDamage(power, Defense);
        DrawHitDamageUI(damage, attacker);
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public override IEnumerator DestroyEnemy(PlayerBase player)
    {
        if (!isDead)
        {
            isDead = true;
            OnDead();
            yield break;
        }
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
        //SetNearTarget();
        //foreach (var aimTransform in aimTransformList)
        //{
        //    aimTransform.GetComponent<EnemyProjectileChecker>().DrawProjectileRayGizmo(target);
        //}

        // 射線の可動域範囲内にプレイヤーがいるか
        //foreach (var aimTransform in aimTransformList)
        //{
        //    var a = aimTransform.GetComponent<EnemyProjectileChecker>().GetNearPlayerInSight(Players);
        //    string log = $"[{aimTransform.gameObject.name}] プレイヤーの視認：{a != null}";
        //    Debug.Log(log);
        //}
    }

    #endregion
}
