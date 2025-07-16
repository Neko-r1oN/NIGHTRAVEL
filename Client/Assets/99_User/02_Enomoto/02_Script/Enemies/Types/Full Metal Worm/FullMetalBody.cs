//**************************************************
//  [�{�X] �t�����^�����[���̑̂̊Ǘ��N���X
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    FullMetalWorm worm;
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
        cancellCoroutines.Add(StartCoroutine(RangeAttack()));
    }

    /// <summary>
    /// �������U���J�n
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
        int generatedEnemiesCnt = worm.GeneratedEnemies.Count;

        GameObject nearPlayer = GetNearPlayer();
        if (generatedEnemiesCnt >= worm.GeneratedMax || nearPlayer == null) return false;   // ����ȏ�G�̐������ł��Ȃ� || �߂��Ƀv���C���[�����݂��Ȃ�

        float distToNearPlayer = Vector2.Distance(transform.position, nearPlayer.transform.position);
        bool isPlayerNearby = distToNearPlayer <= worm.DistToPlayerMin;

        // �����ʒu�̋߂��Ƀv���C���[������ && �����ʒu���X�e�[�W�͈͓̔� && �����ʒu���ǂɖ��܂��Ă��Ȃ��ꍇ
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
    /// �U�R�G�𕡐���������R���[�`��
    /// </summary>
    IEnumerator GenerateEnemeiesCoroutine(int maxEnemies)
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            Random.InitState(System.DateTime.Now.Millisecond + i);  // �����̃V�[�h�l���X�V
            float time = Random.Range(0f, 0.5f);
            yield return new WaitForSeconds(time);
            if (worm.GeneratedEnemies.Count >= worm.GeneratedMax) yield break;  // ���ɐ�������ɒB���Ă���ꍇ

            // �����ɐ������鏈�� && �n�b�`���J���A�j���[�V����####################################
            Random.InitState(System.DateTime.Now.Millisecond);  // �����̃V�[�h�l���X�V
            worm.GeneratedEnemies.Add(GenerateEnemy(transform.position));
        }
    }

    /// <summary>
    /// �U�R�G�𐶐����鏈��
    /// </summary>
    GameObject GenerateEnemy(Vector2 point)
    {
        var enemyObj = Instantiate(worm.EnemyPrefabs[(int)Random.Range(0, worm.EnemyPrefabs.Count)], point, Quaternion.identity).gameObject;
        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();

        if ((int)Random.Range(0, 2) == 0) enemy.Flip();    // �m���Ō������ς��
        enemy.TransparentSprites();
        enemy.Players = GetAlivePlayers();
        return enemyObj;
    }

    #endregion

    #region �q�b�g�����֘A

    /// <summary>
    /// ���S����Ƃ��ɌĂ΂�鏈������
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        SetAnimId((int)ANIM_ID.Dead);
        StopAllCoroutines();
        gunPsControllerList.ForEach(item => { item.StopShooting(); });
    }

    /// <summary>
    /// �_���[�W�K�p����
    /// </summary>
    /// <param name="power"></param>
    /// <param name="attacker"></param>
    /// <param name="effectTypes"></param>
    public override void ApplyDamage(int power, Transform attacker = null, bool drawDmgText = true, params StatusEffectController.EFFECT_TYPE[] effectTypes)
    {
        attacker = null;
        base.ApplyDamage(power, attacker, true, effectTypes);

        // �{�̂�HP�����
        worm.ApplyDamage(power, attacker, false);

        // ��_���[�W�ʂ�\������
        var damage = CalculationLibrary.CalcDamage(power, Defense);
        DrawHitDamageUI(damage, attacker);
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

    #endregion

    #region �`�F�b�N�����֘A

    /// <summary>
    /// ���o�͈͂̕`�揈��
    /// </summary>
    protected override void DrawDetectionGizmos()
    {
        // �U���J�n����
        Gizmos.color = Color.blue;
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
        //    string log = $"[{aimTransform.gameObject.name}] �v���C���[�̎��F�F{a != null}";
        //    Debug.Log(log);
        //}
    }

    #endregion
}
