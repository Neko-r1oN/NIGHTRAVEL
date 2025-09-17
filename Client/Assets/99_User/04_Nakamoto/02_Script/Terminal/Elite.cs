//--------------------------------------------------------------
// �G���[�g�o���^�[�~�i�� [ Elite.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Elite : TerminalBase
{
    //--------------------------------
    // �t�B�[���h

    [SerializeField] private Transform minSpawnPos;
    [SerializeField] private Transform maxSpawnPos;

    //--------------------------------
    // ���\�b�h

    /// <summary>
    /// ��������
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // �[���̎�ʂ�ݒ�
        terminalType = EnumManager.TERMINAL_TYPE.Elite;
    }

    /// <summary>
    /// �N������
    /// </summary>
    public override void BootTerminal()
    {
        isUsed = true; // �[���g�p���ɂ���

        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID].State = EnumManager.TERMINAL_STATE.Active;
        else
        {   // �I�t���C�����
            InvokeRepeating("CountDown", 1, 1);
            SpawnManager.Instance.TerminalGenerateEnemy(SPAWN_ENEMY_NUM, terminalID,
                new Vector2(minSpawnPos.position.x, minSpawnPos.position.y), new Vector2(maxSpawnPos.position.x, maxSpawnPos.position.y), true);
            return;
        }

        // �}�X�^�[�̏ꍇ��CountDown���J�n�B���̑��͏����J�E���g��ݒ�
        if (RoomModel.Instance.IsMaster)
        {
            InvokeRepeating("CountDown", 1, 1);
            SpawnManager.Instance.TerminalGenerateEnemy(SPAWN_ENEMY_NUM, terminalID,
                new Vector2(minSpawnPos.position.x, minSpawnPos.position.y), new Vector2(maxSpawnPos.position.x, maxSpawnPos.position.y), true);
        }
        else
            timerText.text = currentTime.ToString();
    }

    /// <summary>
    /// ���s����
    /// </summary>
    public override void FailureTerminal()
    {
        base.FailureTerminal();

        // ���g�̒[�����琶�����ꂽ�G�̍폜
        CharacterManager.Instance.DeleteTerminalEnemy(terminalID);
    }
}
