//**************************************************
//  �G�l�~�[�̎���͈͊m�F�N���X
//  Author:r-enomoto
//**************************************************
using System.Collections.Generic;
using UnityEngine;

public class EnemySightChecker : MonoBehaviour
{
    [SerializeField] float viewAngleMax = 65;
    [SerializeField] float viewDistMax = 6f;
    LayerMask targetLayerMask;

    private void Start()
    {
        // ���F����Layer [�v���C���[�A�n�ʁE��]
        targetLayerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("BlinkPlayer") | LayerMask.GetMask("Player");
    }

    /// <summary>
    /// �^�[�Q�b�g�����F�ł��Ă��邩�ǂ���
    /// </summary>
    /// <returns></returns>
    public bool IsTargetVisible()
    {
        GameObject target = GetComponent<EnemyBase>().Target;
        if (!target) return false;

        Vector2 dirToTarget = target.transform.position - transform.position;
        Vector2 angleVec = new Vector2(TransformHelper.GetFacingDirection(transform), 0);
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
        GameObject target = GetComponent<EnemyBase>().Target;
        Vector2 dirToTarget = target.transform.position - transform.position;
        float dist = dirToTarget.magnitude;
        float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        RaycastHit2D hit2D = Physics2D.Raycast(transform.position, dirToTarget, dist, targetLayerMask);

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
        foreach (GameObject player in GetComponent<EnemyBase>().Players)
        {
            Vector2 dirToTarget = player.transform.position - transform.position;
            Vector2 angleVec = new Vector2(TransformHelper.GetFacingDirection(transform), 0);
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
        List<GameObject> players = GetComponent<EnemyBase>().Players;
        GameObject target = GetComponent<EnemyBase>().Target;
        if (players.Count > 0)
        {
            foreach (GameObject player in players)
            {
                Vector2 dirToTarget = player.transform.position - transform.position;
                Vector2 angleVec = new Vector2(TransformHelper.GetFacingDirection(transform), 0);
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
}
