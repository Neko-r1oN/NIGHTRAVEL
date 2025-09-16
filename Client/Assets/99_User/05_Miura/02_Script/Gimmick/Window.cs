//====================
// �����t�@���̃X�N���v�g
// Aouther:y-miura
// Date:2025/07/08
//====================

using System.Collections;
using UnityEngine;

public class Window : GimmickBase
{
    [SerializeField] GameObject windObj;
    bool isWind;

    // �}�X�^�N���C�A���g�����g�ɐ؂�ւ�����Ƃ��p
    const float repeatRate = 5f;
    float timer = 0;

    void Start()
    {
        // �I�t���C���� or �}���`�v���C���Ɏ��g���}�X�^�N���C�A���g�̏ꍇ
        if(!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster) 
        {
            InvokeRepeating("RequestActivateGimmick", 0.1f, repeatRate);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    /// <summary>
    /// ���𗬂�����~�߂��肷�鏈��
    /// </summary>
    public void SendWind()
    {
        timer = 0;
        if (isWind==true)
        {
            windObj.SetActive(false);
            isWind=false;
        }
        else if(isWind==false)
        { 
            windObj.SetActive(true);
            isWind=true;
        }
    }

    /// <summary>
    /// �M�~�b�N�N�����N�G�X�g
    /// </summary>
    void RequestActivateGimmick()
    {
        TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
    }

    /// <summary>
    /// �M�~�b�N�N������
    /// </summary>
    public override void TurnOnPower()
    {
        SendWind();
    }

    /// <summary>
    /// �M�~�b�N�ċN������
    /// </summary>
    public override void Reactivate()
    {
        if(timer > repeatRate) timer = repeatRate;
        InvokeRepeating("RequestActivateGimmick", repeatRate - timer, repeatRate);
    }
}
