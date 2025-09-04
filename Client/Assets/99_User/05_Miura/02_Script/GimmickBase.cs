using UnityEngine;

abstract public class GimmickBase : MonoBehaviour
{
    protected bool isBoot = false;    // true�FON, false�FOFF
    public bool IsBoot {  get { return isBoot; } set { isBoot = value; } }

    /// <summary>
    /// �M�~�b�N�̋N��
    /// </summary>
    /// <param name="triggerID"></param>
    public virtual void TurnOnPower(int triggerID)
    {
        isBoot = true;
    }

    /// <summary>
    /// �M�~�b�N�N�����N�G�X�g
    /// </summary>
    /// <param name="player">�N�������v���C���[</param>
    public async void TurnOnPowerRequest(GameObject player)
    {
        // �I�t���C���p
        if (!RoomModel.Instance)
        {
            TurnOnPower(0);
        }
        // �}���`�v���C�� && �N�������l���������g�̏ꍇ
        else if (RoomModel.Instance && player == CharacterManager.Instance.PlayerObjSelf)
        {
            // �T�[�o�[�ɑ΂��ă��N�G�X�g����
        }
    }

    /// <summary>
    /// �}�X�^�N���C�A���g�ɂ��M�~�b�N�N��
    /// ���Ώۂ̃M�~�b�N�FSawBlade��Burn�ȂǁA���Ԋu�œ��삷��M�~�b�N
    /// </summary>
    public virtual void TuenOnPowerByMaster()
    {
        // �I�t���C���� || �}���`�v���C�������g���}�X�^�N���C�A���g�̏ꍇ
        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            TurnOnPower(0);
        }
    }

    // ��ō폜
    abstract public void TruggerRequest();

}
