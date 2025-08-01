//**************************************************
//  [�{�X] �t�����^�����[���̑̂̊Ǘ��N���X
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
    /// �A�j���[�V����ID
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
    /// �Ǘ�����R���[�`���̎��
    /// </summary>
    public enum COROUTINE
    {
        RangeAttack,
        GenerateEnemeiesCoroutine,
    }

    /// <summary>
    /// �����̎��
    /// </summary>
    public enum ROLE_TYPE
    {
        None,
        Spawner,    // �U�R�G����
        Attacker    // �U��
    }

    [SerializeField] 
    ROLE_TYPE roleType;
    public ROLE_TYPE RoleType { get { return roleType; } }

    #region �R���|�[�l���g�֘A
    [Foldout("�R���|�[�l���g")]
    [SerializeField] FullMetalWorm worm;
    #endregion

    #region �U���֘A
    [Foldout("�U���֘A")]
    [SerializeField]
    List<Transform> aimTransformList;
    [Foldout("�U���֘A")]
    [SerializeField]
    List<GunParticleController> gunPsControllerList;
    [Foldout("�U���֘A")]
    [SerializeField]
    float gunBulletWidth = 0;
    [Foldout("�U���֘A")]
    [SerializeField]
    float aimRotetionSpeed = 3f;
    #endregion

    #region �e�N�X�`���E�A�j���[�V�����֘A
    [Foldout("�e�N�X�`���E�A�j���[�V����")]
    [SerializeField] Animator selfJointAnimator;    // ���g�ɕt���Ă��鎕�Ԃ�Animator
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = false;
    }

    /// <summary>
    /// �s���p�^�[�����s����
    /// </summary>
    protected override void DecideBehavior(){}

    /// <summary>
    /// ���[���ɉ������s�������s����R���[�`��
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

    #region �U�������֘A

    /// <summary>
    /// �U������
    /// </summary>
    void Attack()
    {
        // ���s���Ă��Ȃ���΁A�������U���̃R���[�`�����J�n
        string key = COROUTINE.RangeAttack.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(RangeAttack(() => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// �������U���J�n
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

                // ���񎞂ɂ̂ݏ���
                if (!targetList.ContainsKey(aimTransform)) 
                    targetList.Add(aimTransform, projectileChecker.GetNearPlayerInSight(Players, true));

                // �^�[�Q�b�g������������A�^�[�Q�b�g���Đݒ�
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

        // �e�����̊p�x�ɖ߂�
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

    #region �G�̐����֘A

    /// <summary>
    /// �U�R�G�𕡐���������R���[�`���̎��s
    /// </summary>
    bool RunEnemySpawn()
    {
        isAttacking = true;
        bool isGenerateSucsess = false;
        int generatedEnemiesCnt = worm.GeneratedEnemyCnt;

        GameObject nearPlayer = GetNearPlayer();
        if (generatedEnemiesCnt >= worm.GeneratedMax || nearPlayer == null) return false;   // ����ȏ�G�̐������ł��Ȃ� || �߂��Ƀv���C���[�����݂��Ȃ�

        float distToNearPlayer = Vector2.Distance(transform.position, nearPlayer.transform.position);
        bool isPlayerNearby = distToNearPlayer <= worm.DistToPlayerMin;

        // �����ʒu�̋߂��Ƀv���C���[������ && �����ʒu���X�e�[�W�͈͓̔� && �����ʒu���ǂɖ��܂��Ă��Ȃ��ꍇ
        if (isPlayerNearby
            && TransformUtils.IsWithinBounds(transform, SpawnManager.Instance.StageMinPoint, SpawnManager.Instance.StageMaxPoint)
            && !Physics2D.OverlapCircle(transform.position, worm.TerrainCheckRane, terrainLayerMask))
        {
            isGenerateSucsess = true;
            int maxEnemies = UnityEngine.Random.Range(1, 3);
            if (generatedEnemiesCnt + maxEnemies > worm.GeneratedMax) maxEnemies = worm.GeneratedMax - generatedEnemiesCnt;

            // ���s���Ă��Ȃ���΁A�U�R�G�����̃R���[�`�����J�n
            string key = COROUTINE.GenerateEnemeiesCoroutine.ToString();
            if (!ContaintsManagedCoroutine(key))
            {
                Coroutine coroutine = StartCoroutine(GenerateEnemeiesCoroutine(maxEnemies, () => {
                    SetAnimId((int)ANIM_ID.Close);  // �n�b�`������A�j���[�V����
                    RemoveCoroutineByKey(key); 
                }));
                managedCoroutines.Add(key, coroutine);
            }
        }

        return isGenerateSucsess;
    }

    /// <summary>
    /// �U�R�G�𕡐���������R���[�`��
    /// </summary>
    IEnumerator GenerateEnemeiesCoroutine(int maxEnemies, Action onFinished)
    {
        SetAnimId((int)ANIM_ID.Open);  // �n�b�`���J���A�j���[�V����
        List<SpawnEnemyData> spawnDatas = new List<SpawnEnemyData>();
        for (int i = 0; i < maxEnemies; i++)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + i);  // �����̃V�[�h�l���X�V
            float time = UnityEngine.Random.Range(0f, 0.5f);
            yield return new WaitForSeconds(time);

            // ���ɐ�������ɒB���Ă���ꍇ�͐������I��
            if (worm.GeneratedEnemyCnt >= worm.GeneratedMax) break;

            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            spawnDatas.Add(EmitEnemy(transform.position));
            worm.GeneratedEnemyCnt++;
        }

        if (spawnDatas.Count > 0) GenerateEnemy(spawnDatas.ToArray());
        onFinished?.Invoke();
    }

    /// <summary>
    /// ��������G�̒��I����
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
    /// �G�𐶐����鏈��
    /// </summary>
    void GenerateEnemy(SpawnEnemyData[] spawnEnemyDatas)
    {
        SpawnManager.Instance.SpawnEnemyRequest(spawnEnemyDatas);
        var enemyObjs = CharacterManager.Instance.GetEnemiesBySpawnType(SPAWN_ENEMY_TYPE.ByManager);

        for (int i = 0; i < enemyObjs.Count; i++)
        {
            var enemy = enemyObjs[i].GetComponent<EnemyBase>();
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + i);
            if ((int)UnityEngine.Random.Range(0, 2) == 0) enemy.Flip();    // �m���Ō������ς��
            enemy.Players = GetAlivePlayers();
            enemy.Target = GetNearPlayer(enemy.transform.position);
        }
    }

    #endregion

    #region �q�b�g�����֘A

    /// <summary>
    /// ���S����Ƃ��ɌĂ΂�鏈������
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
    /// �_���[�W�K�p����
    /// </summary>
    /// <param name="power"></param>
    /// <param name="attacker"></param>
    /// <param name="effectTypes"></param>
    public override void ApplyDamage(int power, GameObject attacker = null, bool drawDmgText = true, params DEBUFF_TYPE[] effectTypes)
    {
        attacker = null;
        base.ApplyDamage(power, attacker, true, effectTypes);

        // �{�̂�HP�����
        worm.ApplyDamage(power, attacker, false);

        // ��_���[�W�ʂ�\������
        var damage = CalculationLibrary.CalcDamage(power, Defense);
        DrawHitDamageUI(damage);
    }

    /// <summary>
    /// ���S����
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
    /// �f�X�|�[��������
    /// </summary>
    public void Despown()
    {
        selfJointAnimator.SetInteger("animation_id", (int)ANIM_ID.Despown);
        SetAnimId((int)ANIM_ID.Despown);
    }

    #endregion

    #region �e�N�X�`���E�A�j���[�V�����֘A

    /// <summary>
    /// �X�|�[���A�j�����[�V�����J�n��
    /// </summary>
    public void OnSpawnAnimEventByBody()
    {
        OnSpawnAnimEvent();
    }

    /// <summary>
    /// �X�|�[���A�j���[�V�������I�������Ƃ�
    /// </summary>
    public void OnEndSpawnAnimEventByBody()
    {
        OnEndSpawnAnimEvent();
    }

    #endregion

    #region �`�F�b�N�����֘A

    /// <summary>
    /// ���o�͈͂̕`�揈��
    /// </summary>
    protected override void DrawDetectionGizmos()
    {
        // �U���J�n����
        Gizmos.color = UnityEngine.Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackDist);

        // �U���͈�
        //if (meleeAttackCheck)
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawWireSphere(meleeAttackCheck.transform.position, meleeAttackRange);
        //}

        // �ː�
        //SetNearTarget();
        //foreach (var aimTransform in aimTransformList)
        //{
        //    aimTransform.GetComponent<EnemyProjectileChecker>().DrawProjectileRayGizmo(target);
        //}

        // �ː��̉���͈͓��Ƀv���C���[�����邩
        //foreach (var aimTransform in aimTransformList)
        //{
        //    var a = aimTransform.GetComponent<EnemyProjectileChecker>().GetNearPlayerInSight(Players);
        //    string log = $"[{aimTransform.Object.name}] �v���C���[�̎��F�F{a != null}";
        //    Debug.Log(log);
        //}
    }

    #endregion
}
