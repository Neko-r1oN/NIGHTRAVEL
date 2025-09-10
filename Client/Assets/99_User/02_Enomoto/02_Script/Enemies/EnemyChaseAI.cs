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
    [SerializeField] bool canDrawGizmo = false;
    [SerializeField] float checkRange = 1;
    [SerializeField] float rndMoveRange = 3;

    void Start()
    {
        sightChecker = GetComponent<EnemySightChecker>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = GetComponent<EnemyBase>().MoveSpeed * 1.2f;
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
        agent.destination = target.transform.position + destinationOffset;
    }

    /// <summary>
    /// 目標地点に向かって移動する処理
    /// </summary>
    public void DoMove(Vector2 targetPos)
    {
        if (!agent) Start();
        agent.ResetPath();
        agent.destination = targetPos;
    }

    /// <summary>
    /// 障害物がないランダムな場所へ移動する
    /// </summary>
    public void DoRndMove()
    {
        LayerMask tarrainLayer = GetComponent<EnemyBase>().TerrainLayerMask;

        for (int i = 0; i < 100; i++)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + i);  // 乱数のシード値を更新
            float rndX = Random.Range(-rndMoveRange, rndMoveRange);
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + i + 1);  // 乱数のシード値を更新
            float rndY = Random.Range(-rndMoveRange, rndMoveRange);

            Vector2 targetPos = new Vector2(rndX, rndY) + (Vector2)transform.position;
            if (!Physics2D.OverlapCircle(targetPos, checkRange, tarrainLayer)
                && !Physics2D.OverlapCircle(targetPos, checkRange, LayerMask.GetMask("Player")))
            {
                DoMove(targetPos);
                break;
            }
        }
    }

    /// <summary>
    /// 追跡を一時停止
    /// </summary>
    public void Stop()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void OnDrawGizmos()
    {
        if (!canDrawGizmo) return;

        // 検出範囲
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRange);

        // 移動範囲
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rndMoveRange);
    }
}
