//**************************************************
//  [ボス] フルメタルワームの体の管理クラス
//  Author:r-enomoto
//**************************************************
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

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
        Despown
    }

    /// <summary>
    /// 管理するコルーチンの種類
    /// </summary>
    public enum COROUTINE
    {
        RangeAttack,
        GenerateEnemeiesCoroutine,
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
    [Foldout("コンポーネント")]
    [SerializeField] FullMetalWorm worm;
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

    #region テクスチャ・アニメーション関連
    [Foldout("テクスチャ・アニメーション")]
    [SerializeField] Animator selfJointAnimator;    // 自身に付いている歯車のAnimator
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = false;
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
        // 実行していなければ、遠距離攻撃のコルーチンを開始
        string key = COROUTINE.RangeAttack.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(RangeAttack(() => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// 遠距離攻撃開始
    /// </summary>
    /// <returns></returns>
    IEnumerator RangeAttack(Action onFinished)
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

        onFinished?.Invoke();
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
        int generatedEnemiesCnt = worm.GeneratedEnemyCnt;

        GameObject nearPlayer = GetNearPlayer();
        if (generatedEnemiesCnt >= worm.GeneratedMax || nearPlayer == null) return false;   // これ以上敵の生成ができない || 近くにプレイヤーが存在しない

        float distToNearPlayer = Vector2.Distance(transform.position, nearPlayer.transform.position);
        bool isPlayerNearby = distToNearPlayer <= worm.DistToPlayerMin;

        // 生成位置の近くにプレイヤーがいる && 生成位置がステージの範囲内 && 生成位置が壁に埋まっていない場合
        if (isPlayerNearby
            && TransformUtils.IsWithinBounds(transform, SpawnManager.Instance.StageMinPoint, SpawnManager.Instance.StageMaxPoint)
            && !Physics2D.OverlapCircle(transform.position, worm.TerrainCheckRane, terrainLayerMask))
        {
            isGenerateSucsess = true;
            int maxEnemies = UnityEngine.Random.Range(1, 3);
            if (generatedEnemiesCnt + maxEnemies > worm.GeneratedMax) maxEnemies = worm.GeneratedMax - generatedEnemiesCnt;

            // 実行していなければ、ザコ敵生成のコルーチンを開始
            string key = COROUTINE.GenerateEnemeiesCoroutine.ToString();
            if (!ContaintsManagedCoroutine(key))
            {
                Coroutine coroutine = StartCoroutine(GenerateEnemeiesCoroutine(maxEnemies, () => {
                    SetAnimId((int)ANIM_ID.Close);  // ハッチが閉じるアニメーション
                    RemoveCoroutineByKey(key); 
                }));
                managedCoroutines.Add(key, coroutine);
            }
        }

        return isGenerateSucsess;
    }

    /// <summary>
    /// ザコ敵を複数生成するコルーチン
    /// </summary>
    IEnumerator GenerateEnemeiesCoroutine(int maxEnemies, Action onFinished)
    {
        SetAnimId((int)ANIM_ID.Open);  // ハッチが開くアニメーション
        List<SpawnEnemyData> spawnDatas = new List<SpawnEnemyData>();
        for (int i = 0; i < maxEnemies; i++)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + i);  // 乱数のシード値を更新
            float time = UnityEngine.Random.Range(0f, 0.5f);
            yield return new WaitForSeconds(time);

            // 既に生成上限に達している場合は生成を終了
            if (worm.GeneratedEnemyCnt >= worm.GeneratedMax) break;

            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            spawnDatas.Add(EmitEnemy(transform.position));
            worm.GeneratedEnemyCnt++;
        }

        if (spawnDatas.Count > 0) GenerateEnemy(spawnDatas.ToArray());
        onFinished?.Invoke();
    }

    /// <summary>
    /// 生成する敵の抽選処理
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    SpawnEnemyData EmitEnemy(Vector2 point)
    {
        var spawnType = SPAWN_ENEMY_TYPE.ByWorm;
        var emitResult = SpawnManager.Instance.EmitEnemy(ENEMY_TYPE.CyberDog_ByWorm, ENEMY_TYPE.Drone_ByWorm);
        return SpawnManager.Instance.CreateSpawnEnemyData(new EnemySpawnEntry(emitResult, point, Vector3.one), spawnType);
    }

    /// <summary>
    /// 敵を生成する処理
    /// </summary>
    void GenerateEnemy(SpawnEnemyData[] spawnEnemyDatas)
    {
        SpawnManager.Instance.SpawnEnemyRequest(spawnEnemyDatas);
        var enemyObjs = CharacterManager.Instance.GetEnemiesBySpawnType(SPAWN_ENEMY_TYPE.ByManager);

        for (int i = 0; i < enemyObjs.Count; i++)
        {
            var enemy = enemyObjs[i].GetComponent<EnemyBase>();
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + i);
            if ((int)UnityEngine.Random.Range(0, 2) == 0) enemy.Flip();    // 確率で向きが変わる
            enemy.Players = GetAlivePlayers();
            enemy.Target = GetNearPlayer(enemy.transform.position);
        }
    }

    #endregion

    #region ヒット処理関連

    /// <summary>
    /// 死亡するときに呼ばれる処理処理
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        selfJointAnimator.SetInteger("animation_id", (int)ANIM_ID.Dead);
        SetAnimId((int)ANIM_ID.Dead);
        StopAllManagedCoroutines();
        gunPsControllerList.ForEach(item => { item.StopShooting(); });
    }

    /// <summary>
    /// ダメージ適用処理
    /// </summary>
    /// <param name="power"></param>
    /// <param name="attacker"></param>
    /// <param name="effectTypes"></param>
    public override void ApplyDamage(int power, GameObject attacker = null, bool drawDmgText = true, params DEBUFF_TYPE[] effectTypes)
    {
        attacker = null;
        base.ApplyDamage(power, attacker, true, effectTypes);

        // 本体のHPも削る
        worm.ApplyDamage(power, attacker, false);

        // 被ダメージ量を表示する
        var damage = CalculationLibrary.CalcDamage(power, Defense);
        DrawHitDamageUI(damage);
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

    /// <summary>
    /// デスポーンさせる
    /// </summary>
    public void Despown()
    {
        selfJointAnimator.SetInteger("animation_id", (int)ANIM_ID.Despown);
        SetAnimId((int)ANIM_ID.Despown);
    }

    #endregion

    #region テクスチャ・アニメーション関連

    /// <summary>
    /// スポーンアニメメーション開始時
    /// </summary>
    public void OnSpawnAnimEventByBody()
    {
        OnSpawnAnimEvent();
    }

    /// <summary>
    /// スポーンアニメーションが終了したとき
    /// </summary>
    public void OnEndSpawnAnimEventByBody()
    {
        OnEndSpawnAnimEvent();
    }

    #endregion

    #region チェック処理関連

    /// <summary>
    /// 検出範囲の描画処理
    /// </summary>
    protected override void DrawDetectionGizmos()
    {
        // 攻撃開始距離
        Gizmos.color = UnityEngine.Color.blue;
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
        //    string log = $"[{aimTransform.Object.name}] プレイヤーの視認：{a != null}";
        //    Debug.Log(log);
        //}
    }

    #endregion
}
