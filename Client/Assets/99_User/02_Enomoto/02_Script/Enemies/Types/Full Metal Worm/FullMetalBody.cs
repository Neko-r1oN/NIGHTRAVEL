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

    FullMetalWorm worm;

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
    /// �s���p�^�[�����s����
    /// </summary>
    protected override void DecideBehavior(){}

    /// <summary>
    /// ���[���ɉ������s�������s����R���[�`��
    /// </summary>
    /// <returns></returns>
    public IEnumerator ActByRoleTypeCoroutine()
    {
        yield return null;
    }

    #region �U�������֘A

    /// <summary>
    /// �U������
    /// </summary>
    public void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        cancellCoroutines.Add(StartCoroutine(RangeAttack()));
    }

    /// <summary>
    /// �������U���J�n
    /// </summary>
    /// <returns></returns>
    IEnumerator RangeAttack()
    {
        gunPsControllerList.ForEach(item => { item.StartShooting(); });

        float time = 0;
        float waitSec = 0.05f;
        while (time < shotsPerSecond)
        {
            // �^�[�Q�b�g���ǐՔ͈͊O || �^�[�Q�b�g�����݂��Ȃ��ꍇ
            if (target && disToTarget > trackingRange || !target) SetNearTarget();

            // �ǐՔ͈͓��̃^�[�Q�b�g�̂�������Ɍ������ăG�C��
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

        // �e�����̊p�x�ɖ߂�
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

    #region �G�̐����֘A

    /// <summary>
    /// �U�R�G�𕡐���������R���[�`���̎��s
    /// </summary>
    public bool RunEnemySpawn()
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
            int maxEnemies = Random.Range(1, 4);
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
            Random.InitState(System.DateTime.Now.Millisecond);  // �����̃V�[�h�l���X�V
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
        SetNearTarget();
        foreach (var aimTransform in aimTransformList)
        {
            aimTransform.GetComponent<EnemyProjectileChecker>().DrawProjectileRayGizmo(gunBulletWidth, true);
        }
    }

    #endregion
}
