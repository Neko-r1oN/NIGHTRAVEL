//**************************************************
//  �G�l�~�[�̔��˕��Ɋւ��锻����Ǘ�����N���X
//  Author:r-enomoto
//**************************************************
using System.Collections.Generic;
using UnityEngine;


public class EnemyProjectileChecker : MonoBehaviour
{
    [SerializeField] Transform enemy;   // �{��

    [SerializeField] Transform aimTransform;
    [SerializeField] Transform leftFireRayPoint;
    [SerializeField] Transform rightFireRayPoint;
    [SerializeField] float gunBulletWidth;
    [SerializeField] float rotetionSpeed;
    LayerMask targetLayerMask;

    [Range(0f, 180f)]
    [SerializeField] float maxAddAngleLeft;    // ��]�p�x�̉���(�����)
    [Range(0f, 180f)]
    [SerializeField] float maxAddAngleRight;   // ��]�p�x�̉���(�E���)
    [Range(0f, 360f)]
    [SerializeField] float initialAngle;   // ��������Z�����l

    private void Start()
    {
        // ���F����Layer [�v���C���[�A�n�ʁE��]
        targetLayerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("BlinkPlayer") | LayerMask.GetMask("Player");
        aimTransform.eulerAngles = new Vector3(0, 0, initialAngle);
    }

    /// <summary>
    /// AIM�̉�]���X�V
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="rotetionSpeed"></param>
    public void RotateAimTransform(Vector3 direction, float rotetionSpeed = 0)
    {
        if (rotetionSpeed == 0) rotetionSpeed = this.rotetionSpeed;
        Quaternion quaternion = Quaternion.Euler(0, 0, ClampAngleToTarget(direction));
        aimTransform.rotation = Quaternion.RotateTowards(aimTransform.rotation, quaternion, rotetionSpeed);

        // ���˕��̕��ɍ��킹�āA�eRay�̎n�_�𒲐�����
        leftFireRayPoint.localPosition = Vector2.left * gunBulletWidth;
        rightFireRayPoint.localPosition = Vector2.right * gunBulletWidth;
    }

    /// <summary>
    /// �^�[�Q�b�g�����ւ̉�]�p�x���A���鐧���͈͓��Ōv�Z���ĕԂ�
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public float ClampAngleToTarget(Vector3 direction)
    {
        var angleRange = GetAdjustedAngleRangeByFacing();

        // ��������^�[�Q�b�g�̃��W�A�����v�Z���ēx�ɕϊ�����
        float targetAngle = Mathf.Atan2(direction.y, direction.x);
        // �p�x���^��̂Ƃ���0�x�Ƃ����p�x�ɏC����(Unity�͏������0�x�ƂȂ邽��)�A-90�Ŕ����v���Ŋp�x����������悤�ɂ���
        targetAngle = targetAngle * Mathf.Rad2Deg - 90;

        // �����p�x�Ƃ̊p�x�������߁A�����p�x���Ɏ��߂�
        float deltaAngle = Mathf.DeltaAngle(angleRange.initialAngle, targetAngle);
        float clampedDelta = Mathf.Clamp(deltaAngle, -angleRange.maxAddAngleLeft, angleRange.maxAddAngleRight);
        return angleRange.initialAngle + clampedDelta;
    }

    /// <summary>
    /// ���˕���RaycastHit2D�𐶐�����
    /// </summary>
    /// <param name="target"></param>
    /// <param name="gunBulletWidth"></param>
    /// <param name="canFollowTargetSimulation">ray�̌������Atarget�Ɍ����ăV���~���[�V���������p�x�ɂ��邩�ǂ���</param>
    /// <param name="angle"></param>
    /// <param name="canUpdateRotation"></param>
    /// <returns></returns>
    (RaycastHit2D canterHit2D, RaycastHit2D leftHit2D, RaycastHit2D rightHit2D) GetProjectileRaycastHit(Transform target, bool canFollowTargetSimulation, float? angle)
    {
        float attackDist = enemy.GetComponent<EnemyBase>().AttackDist;

        Quaternion simulatedRotation = aimTransform.rotation;
        if (canFollowTargetSimulation)
        {
            // �^�[�Q�b�g�̂������
            Vector2 direction = (target.transform.position - aimTransform.position).normalized;
            simulatedRotation = Quaternion.Euler(0f, 0f, ClampAngleToTarget(direction));
        }
        else if (angle != null)
        {
            // �w�肳�ꂽ�p�x�ɐݒ肷��
            simulatedRotation = Quaternion.Euler(0, 0, (float)angle);
        }
        Vector2 rayDirection = simulatedRotation * Vector2.up; // aimTransform���{����������

        // RaycastHit2D���^�[�Q�b�g�̂�������ɐL�΂�
        RaycastHit2D canterHit2D = Physics2D.Raycast(aimTransform.position, rayDirection, attackDist, targetLayerMask);
        RaycastHit2D leftHit2D = Physics2D.Raycast(leftFireRayPoint.position, rayDirection, attackDist, targetLayerMask);
        RaycastHit2D rightHit2D = Physics2D.Raycast(rightFireRayPoint.position, rayDirection, attackDist, targetLayerMask);
        return (canterHit2D, leftHit2D, rightHit2D);
    }

    /// <summary>
    /// [�p�x�̒Ǐ]] ���˕����΂����Ƃ��ł��邩�ǂ���
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public bool CanFireProjectile(GameObject target, bool canFollowTargetSimulation = true)
    {
        return IsProjectileFireable(target, canFollowTargetSimulation);
    }

    /// <summary>
    /// [�p�x�̎w��] ���˕����΂����Ƃ��ł��邩�ǂ���
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public bool CanFireProjectile(GameObject target, float? angle)
    {
        return IsProjectileFireable(target, angle: angle);
    }

    /// <summary>
    /// [�ː�����`�F�b�N] ���˕����΂����Ƃ��ł��邩�]��
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="canFollowTargetSimulation"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    bool IsProjectileFireable(GameObject target, bool canFollowTargetSimulation = true, float? angle = null)
    {
        if (!target) return false;
        var projectileRays = GetProjectileRaycastHit(target.transform, canFollowTargetSimulation, angle);

        // �v���C���[��Ray���������Ă��邩�A�������m���ł��Ȃ������ꍇ��true
        bool resultCenter = projectileRays.canterHit2D && projectileRays.canterHit2D.collider.gameObject.CompareTag("Player");
        bool resultLeft = projectileRays.leftHit2D && projectileRays.leftHit2D.collider.gameObject.CompareTag("Player") || !projectileRays.leftHit2D;
        bool resultRight = projectileRays.rightHit2D && projectileRays.rightHit2D.collider.gameObject.CompareTag("Player") || !projectileRays.rightHit2D;
        return resultCenter && resultLeft && resultRight;
    }

    /// <summary>
    /// �ː��̉���͈͓��Ƀ^�[�Q�b�g�����邩�ǂ���
    /// </summary>
    /// <returns></returns>
    public bool IsTargetInSight(GameObject target)
    {
        var projectileRays = GetProjectileRaycastHit(target.transform, false, null);

        // �v���C���[��Ray���������Ă��邩�A�������m���ł��Ȃ������ꍇ��true
        bool resultCenter = projectileRays.canterHit2D && projectileRays.canterHit2D.collider.gameObject.CompareTag("Player");
        return resultCenter;
    }

    /// <summary>
    /// �ː��̉���͈͓��ōł��߂��v���C���[�̎擾
    /// </summary>
    /// <returns></returns>
    public GameObject GetNearPlayerInSight(List<GameObject> players, bool canFollowTargetSimulation = true)
    {
        float distToPlayer = float.MaxValue;
        GameObject nearPlayer = null;
        foreach (var player in players)
        {
            if (!player || player && player.GetComponent<CharacterBase>().HP <= 0) continue;
            var projectileRays = GetProjectileRaycastHit(player.transform, canFollowTargetSimulation, null);

            // �v���C���[��Ray���������Ă��邩�A�������m���ł��Ȃ������ꍇ��true
            bool resultCenter = projectileRays.canterHit2D && projectileRays.canterHit2D.collider.gameObject.CompareTag("Player");
            bool resultLeft = projectileRays.leftHit2D && projectileRays.leftHit2D.collider.gameObject.CompareTag("Player") || !projectileRays.leftHit2D;
            bool resultRight = projectileRays.rightHit2D && projectileRays.rightHit2D.collider.gameObject.CompareTag("Player") || !projectileRays.rightHit2D;

            // �ː��̉���͈͓� && ��苗�����߂��ꍇ
            if (resultCenter && resultLeft && resultRight && 
                distToPlayer > Vector2.Distance(player.transform.position, transform.position))
            {
                distToPlayer = Vector2.Distance(player.transform.position, transform.position);
                nearPlayer = player;
            }
        }
        return nearPlayer;
    }

    /// <summary>
    /// [�f�o�b�N�p:�p�x���^�[�Q�b�g�Ɍ������ĒǏ]] ���˕��̎ː���`�悷��
    /// </summary>
    /// <param name="projectile"></param>
    public void DrawProjectileRayGizmo(GameObject target, bool canFollowTargetSimulation = true)
    {
        DrawProjectileRay(target, canFollowTargetSimulation);
    }

    /// <summary>
    /// [�f�o�b�N�p:�p�x�̎w��] ���˕��̎ː���`�悷��
    /// </summary>
    /// <param name="projectile"></param>
    public void DrawProjectileRayGizmo(GameObject target, float? angle)
    {
        DrawProjectileRay(target, angle: angle);
    }

    /// <summary>
    /// [Debug�p] ���˕��̎ː���`�悷��
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="followTargetRotation"></param>
    /// <param name="angle"></param>
    void DrawProjectileRay(GameObject target, bool canFollowTargetSimulation = true, float? angle = null)
    {
        if (!target) return;
        var projectileRays = GetProjectileRaycastHit(target.transform, canFollowTargetSimulation, angle);

        // �f�o�b�N�p
        Color rayUpColor = Color.red;
        Color rayDownColor = Color.red;
        if (projectileRays.leftHit2D && !projectileRays.leftHit2D.collider.gameObject.CompareTag("Player"))
        {
            rayUpColor = Color.gray;
        }
        if (projectileRays.rightHit2D && !projectileRays.rightHit2D.collider.gameObject.CompareTag("Player"))
        {
            rayDownColor = Color.gray;
        }

        Vector3 rayDirection = aimTransform.up;
        float attackDist = enemy.GetComponent<EnemyBase>().AttackDist;
        Debug.DrawRay(aimTransform.position, rayDirection * attackDist, rayUpColor);
        Debug.DrawRay(leftFireRayPoint.position, rayDirection * attackDist, rayUpColor);
        Debug.DrawRay(rightFireRayPoint.position, rayDirection * attackDist, rayDownColor);
    }

    public Vector2 GetDirection(float initialAngle, float angle)
    {
        // �^���0�x�ɂ��A��̒l�����Z
        float angleInRadians = (angle + 90 + initialAngle) * Mathf.Deg2Rad;
        // �p�x���g���Č����𐶐�
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }

    /// <summary>
    /// �����ɉ����Ē������ꂽ�ː��̉���i� , ���E��max�j���擾
    /// </summary>
    (float maxAddAngleLeft, float maxAddAngleRight, float initialAngle) GetAdjustedAngleRangeByFacing()
    {
        // �{�̂̃e�N�X�`�������]���Ă�����A�p�x�����]������
        float directionMultiplier = TransformUtils.GetFacingDirection(enemy);
        float maxAddAngleLeft = directionMultiplier == 1 ? this.maxAddAngleLeft : this.maxAddAngleRight;
        float maxAddAngleRight = directionMultiplier == 1 ? this.maxAddAngleRight : this.maxAddAngleLeft;
        float initialAngle = this.initialAngle * directionMultiplier + enemy.transform.eulerAngles.z;  // �{�̂������Ă��郏�[���h�p�x���l��
        return (maxAddAngleLeft, maxAddAngleRight, initialAngle);
    }

    /// <summary>
    /// [�f�o�b�N�p] �ː��̉����`�悷��
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!enemy || enemy && !enemy.GetComponent<EnemyBase>().CanDrawRay) return;

        var adjustedAngleRange = GetAdjustedAngleRangeByFacing();
        float attackDist = enemy.GetComponent<EnemyBase>().AttackDist;
        Debug.DrawRay(aimTransform.position, GetDirection(adjustedAngleRange.initialAngle, -adjustedAngleRange.maxAddAngleLeft).normalized * attackDist, Color.red);
        Debug.DrawRay(aimTransform.position, GetDirection(adjustedAngleRange.initialAngle, adjustedAngleRange.maxAddAngleRight).normalized * attackDist, Color.red);
    }
}
