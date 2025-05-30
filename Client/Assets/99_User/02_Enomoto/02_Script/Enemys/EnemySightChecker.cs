//**************************************************
//  �G�l�~�[�̎���͈͊m�F�N���X
//  Author:r-enomoto
//**************************************************
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class EnemySightChecker : MonoBehaviour
{
    [SerializeField] LayerMask targetLayerMask; // ���F����Layer [�v���C���[�A�n�ʁA��]
    [SerializeField] float viewAngleMax = 45;
    [SerializeField] float viewDistMax = 6f;

    #region �ˏo�|�C���g�֌W
    [SerializeField] Transform aimTransform;
    [SerializeField] Transform leftFireRayPoint;
    [SerializeField] Transform rightFireRayPoint;
    #endregion

    /// <summary>
    /// �^�[�Q�b�g�����F�ł��Ă��邩�ǂ���
    /// </summary>
    /// <returns></returns>
    public bool IsTargetVisible()
    {
        GameObject target = GetComponent<EnemyController>().Target;
        if (!target) return false;

        Vector2 dirToTarget = target.transform.position - transform.position;
        Vector2 angleVec = new Vector2(transform.localScale.x, 0);
        float angle = Vector2.Angle(dirToTarget, angleVec);
        RaycastHit2D hit2D = Physics2D.Raycast(transform.position, dirToTarget, viewDistMax, targetLayerMask);
        return angle <= viewAngleMax && hit2D && hit2D.collider.gameObject.CompareTag("Player");
    }

    /// <summary>
    /// �^�[�Q�b�g�Ƃ̊Ԃɏ�Q�������邩�ǂ���
    /// </summary>
    /// <returns></returns>
    public bool IsObstructed()
    {
        GameObject target = GetComponent<EnemyController>().Target;
        Vector2 dirToTarget = target.transform.position - transform.position;
        float dist = dirToTarget.magnitude;
        float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        RaycastHit2D hit2D = Physics2D.Raycast(transform.position, dirToTarget, dist, targetLayerMask);
        //RaycastHit2D hit2D = Physics2D.BoxCast(transform.position, new Vector2(dist, size.y), angle, dirToTarget, targetLayerMask);

        return hit2D && !hit2D.collider.gameObject.CompareTag("Player");
    }

    /// <summary>
    /// ����͈͓��̃v���C���[�̒�����^�[�Q�b�g���擾����
    /// </summary>
    /// <returns></returns>
    public GameObject GetTargetInSight()
    {
        GameObject target = null;
        float minTargetDist = float.MaxValue;
        foreach (GameObject player in GetComponent<EnemyController>().Players)
        {
            Vector2 dirToTarget = player.transform.position - transform.position;
            Vector2 angleVec = new Vector2(transform.localScale.x, 0);
            float angle = Vector2.Angle(dirToTarget, angleVec);
            RaycastHit2D hit2D = Physics2D.Raycast(transform.position, dirToTarget, viewDistMax, targetLayerMask);

            if (angle <= viewAngleMax && hit2D && hit2D.collider.gameObject.CompareTag("Player"))
            {
                float distTotarget = Vector3.Distance(this.transform.position, player.transform.position);
                if (distTotarget < minTargetDist)
                {
                    minTargetDist = distTotarget;
                    target = player;
                }
            }
        }
        return target;
    }

    /// <summary>
    /// ������`�悷��
    /// </summary>
    /// <param name="players"></param>
    /// <param name="target"></param>
    /// <param name="canChaseTarget"></param>
    public void DrawSightLine(bool canChaseTarget)
    {
        List<GameObject> players = GetComponent<EnemyController>().Players;
        GameObject target = GetComponent<EnemyController>().Target;
        if (players.Count > 0)
        {
            foreach (GameObject player in players)
            {
                Vector2 dirToTarget = player.transform.position - transform.position;
                Vector2 angleVec = new Vector2(transform.localScale.x, 0);
                float angle = Vector2.Angle(dirToTarget, angleVec);
                RaycastHit2D hit2D = Physics2D.Raycast(transform.position, dirToTarget, viewDistMax, targetLayerMask);

                if (canChaseTarget && target && target == player)
                {
                    Debug.DrawRay(transform.position, dirToTarget, Color.red);
                }
                else if (angle <= viewAngleMax && hit2D && hit2D.collider.gameObject.CompareTag("Player"))
                {
                    Debug.DrawRay(transform.position, dirToTarget, Color.red);
                }
                else
                {
                    Debug.DrawRay(transform.position, dirToTarget, Color.cyan);
                }
            }
        }
    }

    /// <summary>
    /// ���˕���RaycastHit2D�𐶐�����
    /// </summary>
    /// <returns></returns>
    (RaycastHit2D canterHit2D, RaycastHit2D leftHit2D, RaycastHit2D rightHit2D) GetProjectileRaycastHit(Transform target, GameObject projectile, bool followTargetRotation, float? angle)
    {
        float attackDist = GetComponent<EnemyController>().AttackDist;
        float projectileHeight = projectile.GetComponent<SpriteRenderer>().bounds.size.y / 2;

        if (followTargetRotation)
        {
            // �^�[�Q�b�g�̂�������Ɍ�����Ǐ]����
            Vector2 direction = target.transform.position - aimTransform.position;
            aimTransform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
        }
        else if (angle != null)
        {
            // �w�肳�ꂽ�p�x�ɐݒ肷��
            aimTransform.localRotation = Quaternion.Euler(0, 0, (float)angle);
        }
        Vector2 rayDirection = aimTransform.up; // aimTransform�������Ă������

        // ���˕��̕��ɍ��킹�āA�eRay�̎n�_�𒲐�����
        leftFireRayPoint.localPosition = Vector2.left * projectileHeight;
        rightFireRayPoint.localPosition = Vector2.right * projectileHeight;

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
    public bool CanFireProjectile(GameObject projectile, bool followTargetRotation = false)
    {
        return IsProjectileFireable(projectile, followTargetRotation);
    }

    /// <summary>
    /// [�p�x�̎w��] ���˕����΂����Ƃ��ł��邩�ǂ���
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public bool CanFireProjectile(GameObject projectile, float? angle = null)
    {
        return IsProjectileFireable(projectile, angle: angle);
    }

    /// <summary>
    /// ���˕����΂����Ƃ��ł��邩�]��
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="followTargetRotation"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    bool IsProjectileFireable(GameObject projectile, bool followTargetRotation = false, float? angle = null)
    {
        GameObject target = GetComponent<EnemyController>().Target;
        if (!target || !projectile) return false;

        var projectileRays = GetProjectileRaycastHit(target.transform, projectile, followTargetRotation, angle);

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
    public void DrawProjectileRayGizmo(GameObject projectile, bool followTargetRotation = false)
    {
        DrawProjectileRay(projectile, followTargetRotation);
    }

    /// <summary>
    /// [�f�o�b�N�p:�p�x�̎w��] ���˕��̎ː���`�悷��
    /// </summary>
    /// <param name="projectile"></param>
    public void DrawProjectileRayGizmo(GameObject projectile, float? angle = null)
    {
        DrawProjectileRay(projectile, angle:  angle);
    }

    /// <summary>
    /// ���˕��̎ː���`�悷��
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="followTargetRotation"></param>
    /// <param name="angle"></param>
    void DrawProjectileRay(GameObject projectile, bool followTargetRotation = false, float? angle = null)
    {
        float projectileHeight = projectile.GetComponent<SpriteRenderer>().bounds.size.y / 2;
        GameObject target = GetComponent<EnemyController>().Target;
        if (!target) return;

        var projectileRays = GetProjectileRaycastHit(target.transform, projectile, followTargetRotation, angle);

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
        float attackDist = GetComponent<EnemyController>().AttackDist;
        Debug.DrawRay(aimTransform.position, rayDirection * attackDist, rayUpColor);
        Debug.DrawRay(leftFireRayPoint.position, rayDirection * attackDist, rayUpColor);
        Debug.DrawRay(rightFireRayPoint.position, rayDirection * attackDist, rayDownColor);
    }
}
