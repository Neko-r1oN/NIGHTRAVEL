//**************************************************
//  エネミーがターゲットを追跡するクラス
//  Author:r-enomoto
//**************************************************
using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseAI : MonoBehaviour
{
    EnemySightChecker sightChecker;
    NavMeshAgent agent;
    [SerializeField] Vector2 offset;

    //------------------
    // 試験用
    //------------------
    //[SerializeField] GameObject targetObj;
    //[SerializeField] float distance = 0;
    Vector3 previousDestination;

    void Start()
    {
        sightChecker = GetComponent<EnemySightChecker>();
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
        // テクスチャが反転した際に、オフセットも反転させる
        float directionMultiplier = Mathf.Clamp(transform.localScale.x, -1, 1);

        // ターゲットを視認できている場合、オフセットを有効にする
        Vector3 destinationOffset = Vector3.zero;
        if (sightChecker.IsTargetVisible())
        {
            destinationOffset = new Vector3((float)offset.x * directionMultiplier, (float)offset.y);
        }

        previousDestination = agent.destination;
        agent.destination = target.transform.position + destinationOffset;
    }

    /// <summary>
    /// 目標地点に向かって移動する処理
    /// </summary>
    public void DoMove(Vector2 targetPos)
    {
        if (!agent) Start();
        previousDestination = agent.destination;
        agent.destination = targetPos;
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
    public void Stop()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }
}
