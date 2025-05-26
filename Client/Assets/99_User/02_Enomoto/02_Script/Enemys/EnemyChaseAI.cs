//**************************************************
//  エネミーがターゲットを追跡するクラス
//  Author:r-enomoto
//**************************************************
using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseAI : MonoBehaviour
{
    NavMeshAgent agent;

    //------------------
    // 試験用
    //------------------
    [SerializeField] GameObject targetObj;
    [SerializeField] float distance = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        //DoChase(targetObj,distance)
    }

    public void DoChase(GameObject target)
    {
        float now = Vector2.Distance(transform.position, target.transform.position);
        if (distance < now)
        {

        }

        agent.destination = target.transform.position;

        // 次の目的地への方向ベクトル？
        //Vector2? vector = new Vector2(
        //    agent.destination.x - transform.position.x,
        //    agent.destination.y - transform.position.y
        //);
    }

    /// <summary>
    /// 追跡を一時停止
    /// </summary>
    public void StopChase()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }
}
