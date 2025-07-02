//**************************************************
//  �G�l�~�[�̔��˕��Ɋւ��锻���S������N���X
//  Author:r-enomoto
//**************************************************
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public class EnemyProjectileChecker : MonoBehaviour
{
    [SerializeField] Transform aimTransform;
    [SerializeField] Transform leftFireRayPoint;
    [SerializeField] Transform rightFireRayPoint;
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
    /// �^�[�Q�b�g�����ւ̉�]�p�x���A���鐧���͈͓��Ōv�Z���ĕԂ�
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public float ClampAngleToTarget(Vector3 direction)
    {
        // �e�N�X�`�������]���Ă�����A�p�x�����]������
        float directionMultiplier = TransformUtils.GetFacingDirection(transform);
        float maxAddAngleLeft = directionMultiplier == 1 ? this.maxAddAngleLeft : this.maxAddAngleRight;
        float maxAddAngleRight = directionMultiplier == 1 ? this.maxAddAngleRight : this.maxAddAngleLeft;
        float initialAngle = this.initialAngle * directionMultiplier;

        // ��������^�[�Q�b�g�̃��W�A�����v�Z���ēx�ɕϊ�����
        float targetAngle = Mathf.Atan2(direction.y, direction.x);
        // �p�x���^��̂Ƃ���0�x�Ƃ����p�x�ɏC����(Unity�͏������0�x�ƂȂ邽��)�A-90�Ŕ����v���Ŋp�x����������悤�ɂ���
        targetAngle = targetAngle * Mathf.Rad2Deg - 90;

        // �����p�x�Ƃ̊p�x�������߁A�����p�x���Ɏ��߂�
        float deltaAngle = Mathf.DeltaAngle(initialAngle, targetAngle);
        float clampedDelta = Mathf.Clamp(deltaAngle, -maxAddAngleLeft, maxAddAngleRight);
        return initialAngle + clampedDelta;
    }

    /// <summary>
    /// ���˕���RaycastHit2D�𐶐�����
    /// </summary>
    /// <returns></returns>
    (RaycastHit2D canterHit2D, RaycastHit2D leftHit2D, RaycastHit2D rightHit2D) GetProjectileRaycastHit(Transform target, float gunBulletWidth, bool followTargetRotation, float? angle, bool canUpdateRotation = true)
    {
        float attackDist = GetComponent<EnemyBase>().AttackDist;

        if (followTargetRotation && canUpdateRotation)
        {
            // �^�[�Q�b�g�̂������
            Vector2 direction = (target.transform.position - aimTransform.position).normalized;
            aimTransform.rotation = Quaternion.Euler(0f, 0f, ClampAngleToTarget(direction));
        }
        else if (angle != null && canUpdateRotation)
        {
            // �w�肳�ꂽ�p�x�ɐݒ肷��
            aimTransform.localRotation = Quaternion.Euler(0, 0, (float)angle);
        }
        Vector2 rayDirection = aimTransform.up; // aimTransform�������Ă������

        if (canUpdateRotation)
        {
            // ���˕��̕��ɍ��킹�āA�eRay�̎n�_�𒲐�����
            leftFireRayPoint.localPosition = Vector2.left * gunBulletWidth;
            rightFireRayPoint.localPosition = Vector2.right * gunBulletWidth;
        }

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
    public bool CanFireProjectile(float gunBulletWidth, bool followTargetRotation = false)
    {
        return IsProjectileFireable(gunBulletWidth, followTargetRotation);
    }

    /// <summary>
    /// [�p�x�̎w��] ���˕����΂����Ƃ��ł��邩�ǂ���
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public bool CanFireProjectile(float gunBulletWidth, float? angle = null)
    {
        return IsProjectileFireable(gunBulletWidth, angle: angle);
    }

    /// <summary>
    /// ���˕����΂����Ƃ��ł��邩�]��
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="followTargetRotation"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    bool IsProjectileFireable(float gunBulletWidth, bool followTargetRotation = false, float? angle = null)
    {
        GameObject target = GetComponent<EnemyBase>().Target;
        if (!target) return false;

        var projectileRays = GetProjectileRaycastHit(target.transform, gunBulletWidth, followTargetRotation, angle);

        // �v���C���[��Ray���������Ă��邩�A�������m���ł��Ȃ������ꍇ��true
        bool resultCenter = projectileRays.canterHit2D && projectileRays.canterHit2D.collider.gameObject.CompareTag("Player");
        bool resultLeft = projectileRays.leftHit2D && projectileRays.leftHit2D.collider.gameObject.CompareTag("Player") || !projectileRays.leftHit2D;
        bool resultRight = projectileRays.rightHit2D && projectileRays.rightHit2D.collider.gameObject.CompareTag("Player") || !projectileRays.rightHit2D;
        return resultCenter && resultLeft && resultRight;
    }

    /// <summary>
    /// [�f�o�b�N�p:�p�x�̒Ǐ]] ���˕��̎ː���`�悷��
    /// </summary>
    /// <param name="projectile"></param>
    public void DrawProjectileRayGizmo(float gunBulletWidth, bool followTargetRotation = false)
    {
        DrawProjectileRay(gunBulletWidth, followTargetRotation);
    }

    /// <summary>
    /// [�f�o�b�N�p:�p�x�̎w��] ���˕��̎ː���`�悷��
    /// </summary>
    /// <param name="projectile"></param>
    public void DrawProjectileRayGizmo(float gunBulletWidth, float? angle = null)
    {
        DrawProjectileRay(gunBulletWidth, angle: angle);
    }

    /// <summary>
    /// [Debug�p] ���˕��̎ː���`�悷��
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="followTargetRotation"></param>
    /// <param name="angle"></param>
    void DrawProjectileRay(float gunBulletWidth, bool followTargetRotation = false, float? angle = null)
    {
        GameObject target = GetComponent<EnemyBase>().Target;
        if (!target) return;

        var projectileRays = GetProjectileRaycastHit(target.transform, gunBulletWidth, followTargetRotation, angle, false);

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
        float attackDist = GetComponent<EnemyBase>().AttackDist;
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
    /// [�f�o�b�N�p] �ː��̉����`�悷��
    /// </summary>
    private void OnDrawGizmos()
    {
        float initialAngle;
        initialAngle = this.initialAngle;

        // �e�N�X�`�������]���Ă�����A�p�x�����]������
        float directionMultiplier = TransformUtils.GetFacingDirection(transform);
        float maxAngleLeft = directionMultiplier == 1 ? this.maxAddAngleLeft : this.maxAddAngleRight;
        float maxAngleRight = directionMultiplier == 1 ? this.maxAddAngleRight : this.maxAddAngleLeft;
        initialAngle *= directionMultiplier;

        float attackDist = GetComponent<EnemyBase>().AttackDist;
        Debug.DrawRay(aimTransform.position, GetDirection(initialAngle, -maxAngleLeft).normalized * attackDist, Color.red);
        Debug.DrawRay(aimTransform.position, GetDirection(initialAngle, maxAngleRight).normalized * attackDist, Color.red);
    }
}
