//**************************************************
//  �G�l�~�[���^�[�Q�b�g��ǐՂ���N���X
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
    /// �ǐՊJ�n
    /// </summary>
    /// <param name="target"></param>
    public void DoChase(GameObject target)
    {
        // �e�N�X�`�������]�����ۂɁA�I�t�Z�b�g�����]������
        float directionMultiplier = Mathf.Clamp(transform.localScale.x, -1, 1);

        // �^�[�Q�b�g�����F�ł��Ă���ꍇ�A�I�t�Z�b�g��L���ɂ���
        Vector3 destinationOffset = Vector3.zero;
        if (sightChecker.IsTargetVisible())
        {
            destinationOffset = new Vector3((float)offset.x * directionMultiplier, (float)offset.y);
        }
        agent.destination = target.transform.position + destinationOffset;
    }

    /// <summary>
    /// �ڕW�n�_�Ɍ������Ĉړ����鏈��
    /// </summary>
    public void DoMove(Vector2 targetPos)
    {
        if (!agent) Start();
        agent.ResetPath();
        agent.destination = targetPos;
    }

    /// <summary>
    /// ��Q�����Ȃ������_���ȏꏊ�ֈړ�����
    /// </summary>
    public void DoRndMove()
    {
        LayerMask tarrainLayer = GetComponent<EnemyBase>().TerrainLayerMask;

        for (int i = 0; i < 100; i++)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + i);  // �����̃V�[�h�l���X�V
            float rndX = Random.Range(-rndMoveRange, rndMoveRange);
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + i + 1);  // �����̃V�[�h�l���X�V
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
    /// �ǐՂ��ꎞ��~
    /// </summary>
    public void Stop()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void OnDrawGizmos()
    {
        if (!canDrawGizmo) return;

        // ���o�͈�
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRange);

        // �ړ��͈�
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rndMoveRange);
    }
}
