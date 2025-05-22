//**************************************************
//  エネミーの視野範囲確認クラス
//  Author:r-enomoto
//**************************************************
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class EnemySightChecker : MonoBehaviour
{
    [SerializeField] LayerMask targetLayerMask; // 視認するLayer [プレイヤー、地面、壁]
    [SerializeField] float viewAngleMax = 45;
    [SerializeField] float viewDistMax = 6f;

    /// <summary>
    /// ターゲットを視認できているかどうか
    /// </summary>
    /// <returns></returns>
    public bool IsTargetVisible(GameObject target)
    {
        if (!target) return false;
        Vector2 dirToTarget = target.transform.position - transform.position;
        Vector2 angleVec = new Vector2(transform.localScale.x, 0);
        float angle = Vector2.Angle(dirToTarget, angleVec);
        RaycastHit2D hit2D = Physics2D.Raycast(transform.position, dirToTarget, viewDistMax, targetLayerMask);
        return angle <= viewAngleMax && hit2D && hit2D.collider.gameObject.CompareTag("Player");
    }

    /// <summary>
    /// ターゲットとの間に障害物があるかどうか
    /// </summary>
    /// <returns></returns>
    public bool IsObstructed(GameObject target)
    {
        Vector2 dirToTarget = target.transform.position - transform.position;
        float dist = dirToTarget.magnitude;
        RaycastHit2D hit2D = Physics2D.Raycast(transform.position, dirToTarget, dist, targetLayerMask);
        return hit2D && !hit2D.collider.gameObject.CompareTag("Player");
    }

    /// <summary>
    /// 視野範囲内のプレイヤーの中からターゲットを取得する
    /// </summary>
    /// <returns></returns>
    public GameObject GetTargetInSight(List<GameObject> players)
    {
        GameObject target = null;
        float minTargetDist = float.MaxValue;

        foreach (GameObject player in players)
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
    /// 視線を描画する
    /// </summary>
    /// <param name="players"></param>
    /// <param name="target"></param>
    /// <param name="canChaseTarget"></param>
    public void DrawSightLine(List<GameObject> players, GameObject target, bool canChaseTarget)
    {
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
                if (angle <= viewAngleMax && hit2D && hit2D.collider.gameObject.CompareTag("Player"))
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
