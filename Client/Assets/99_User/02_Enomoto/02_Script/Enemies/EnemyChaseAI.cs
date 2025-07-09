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

    //------------------
    // �����p
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

        previousDestination = agent.destination;
        agent.destination = target.transform.position + destinationOffset;
    }

    /// <summary>
    /// �ڕW�n�_�Ɍ������Ĉړ����鏈��
    /// </summary>
    public void DoMove(Vector2 targetPos)
    {
        if (!agent) Start();
        previousDestination = agent.destination;
        agent.destination = targetPos;
    }

    /// <summary>
    /// �O��̒n�_�Ɉ����Ԃ�����(�ۗ���)
    /// </summary>
    public void ReturnToPreviousDestination()
    {
        //agent.destination = previousDestination;
    }

    /// <summary>
    /// �ǐՂ��ꎞ��~
    /// </summary>
    public void Stop()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }
}
