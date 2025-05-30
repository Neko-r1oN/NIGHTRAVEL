//**************************************************
//  �G�l�~�[���^�[�Q�b�g��ǐՂ���N���X
//  Author:r-enomoto
//**************************************************
using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseAI : MonoBehaviour
{
    NavMeshAgent agent;

    //------------------
    // �����p
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
    /// �ǐՊJ�n
    /// </summary>
    /// <param name="target"></param>
    public void DoChase(GameObject target)
    {
        previousDestination = agent.destination;
        agent.destination = target.transform.position;
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
    public void StopChase()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }
}
