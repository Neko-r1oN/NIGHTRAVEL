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
    Vector3 previousDestination;

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

    /// <summary>
    /// 追跡開始
    /// </summary>
    /// <param name="target"></param>
    public void DoChase(GameObject target)
    {
        previousDestination = agent.destination;
        agent.destination = target.transform.position;
    }

    /// <summary>
    /// 前回の地点に引き返す処理(保留中)
    /// </summary>
    public void ReturnToPreviousDestination()
    {
        //agent.destination = previousDestination;
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
