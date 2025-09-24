//======================
// �M�~�b�N�S�̂̐e�N���X
// Auther:y-miura
// Date:2025/07/01
//======================

using DG.Tweening;
using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;

abstract public class GimmickBase : MonoBehaviour
{
    [SerializeField]
    bool triggerOnce;   // �N���ł���̂���x���肩�ǂ���

    [SerializeField]
    bool requiresReactivation;  // �}�X�^�N���C�A���g�ɐ؂�ւ�����Ƃ��ɁA�ċN�����K�v���ǂ���

    // ���ʗpID
    string uniqueId;
    public string UniqueId {  get { return uniqueId; } set { uniqueId = value; } }

    // �M�~�b�N�̏�� (true�FON, false�FOFF)
    protected bool isBoot = false;
    public bool IsBoot { get { return isBoot; } set { isBoot = value; } }

    /// <summary>
    /// �M�~�b�N�̋N��
    /// </summary>
    /// <param name="triggerID"></param>

    public virtual void TurnOnPower()
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
            TurnOnPower();
        }
        // �}���`�v���C�� && �N�������l���������g�̏ꍇ
        else if (RoomModel.Instance && player == CharacterManager.Instance.PlayerObjSelf)
        {
            // �T�[�o�[�ɑ΂��ă��N�G�X�g����
            await RoomModel.Instance.BootGimmickAsync(uniqueId, triggerOnce);
        }
    }

    /// <summary>
    /// [�}�X�^�N���C�A���g�����g�ɐ؂�ւ�����Ƃ��ɌĂ΂��]
    /// �M�~�b�N�ċN������
    /// </summary>
    public virtual void Reactivate()
    {
        Debug.Log($"{gameObject.name}���ċN�������B");
    }

    /// <summary>
    /// [���A���^�C�������p] �M�~�b�N�X�V
    /// </summary>
    public virtual void UpdateGimmick(GimmickData gimmickData)
    {
        transform.DOMove(gimmickData.Position, CharacterManager.Instance.UpdateSec).SetEase(Ease.Linear);
    }
}
