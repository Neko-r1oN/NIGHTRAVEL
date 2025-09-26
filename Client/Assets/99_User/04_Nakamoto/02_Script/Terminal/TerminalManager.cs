//--------------------------------------------------------------
// �^�[�~�i���Ǘ��N���X [ TerminalManager.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;

public class TerminalManager : MonoBehaviour
{
    //--------------------------------
    // �t�B�[���h

    #region �O�����ݒ�

    /// <summary>
    /// �[�������ʒu
    /// </summary>
    [SerializeField] private GameObject[] generatePos;

    /// <summary>
    /// �����[���v���n�u
    /// </summary>
    [SerializeField] private GameObject[] terminalPrefabs;

    #endregion

    #region �[�����

    /// <summary>
    /// �����[�����X�g
    /// </summary>
    private List<TerminalData> terminalDatas = new List<TerminalData>();

    /// <summary>
    /// �������ꂽ�[���I�u�W�F�N�g���X�g
    /// </summary>
    private Dictionary<int, GameObject> terminalObjs = new Dictionary<int, GameObject>();

    #endregion

    #region ���Q�Ɨp�v���p�e�B

    public List<TerminalData> TerminalDatas { get { return terminalDatas; } set { terminalDatas = value; } }

    public Dictionary<int, GameObject> TerminalObjs { get { return terminalObjs; } set { terminalObjs = value; } }

    #endregion

    #region Instance

    [Header("Instance")]

    private static TerminalManager instance;

    public static TerminalManager Instance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    //--------------------------------
    // ���\�b�h

    /// <summary>
    /// �N������
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            // �C���X�^���X���������݂��Ȃ��悤�ɁA���ɑ��݂��Ă����玩�g����������
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    private async void Start()
    {
        if (RoomModel.Instance)
        {
            RoomModel.Instance.OnBootedTerminal += this.OnBootedTerminal;
            RoomModel.Instance.OnTerminalsSuccessed += this.OnTerminalsSuccessed;
            RoomModel.Instance.OnTerminalFailured += this.OnTerminalFailured;
            await RoomModel.Instance.AdvancedStageAsync();  //�J�ڊ����̃��N�G�X�g
        }
    }

    /// <summary>
    /// �[���̐�������
    /// </summary>
    /// <param name="list"></param>
    public void SetTerminal(List<TerminalData> list)
    {
        terminalDatas = list;

        if(terminalObjs.Count == 0)
        {
            foreach (var data in list)
            {
                var terminal = Instantiate(terminalPrefabs[(int)data.Type - 1], generatePos[data.ID - 1].transform.position, Quaternion.identity);

                if(data.ID == 2)
                {
                    terminal.transform.GetChild(0).GetComponent<TerminalBase>().TerminalID = data.ID;
                    terminal.transform.GetChild(0).GetComponent<TerminalBase>().TerminalType = data.Type;
                }
                else
                {
                    terminal.GetComponent<TerminalBase>().TerminalID = data.ID;
                    terminal.GetComponent<TerminalBase>().TerminalType = data.Type;
                }

                terminalObjs.Add(data.ID, terminal);
            }
        }
    }

    /// <summary>
    /// ��V�r�o����
    /// </summary>
    /// <param name="termID"></param>
    public void DropRelic(int termID)
    {
        terminalObjs[termID].GetComponent<TerminalBase>().GiveRewardRequest();
    }

    /// <summary>
    /// �w��[���̋N������
    /// </summary>
    /// <param name="id"></param>
    public void OnBootedTerminal(int id)
    {
        terminalDatas[id - 1].State = EnumManager.TERMINAL_STATE.Active;

        if(id == 2)
        {
            GameObject child = terminalObjs[id].transform.GetChild(0).gameObject;
            child.GetComponent<TerminalBase>().BootTerminal();
        }
        else
        {
            if (terminalObjs[id] != null)
            terminalObjs[id].GetComponent<TerminalBase>().BootTerminal();
        }
    }

    /// <summary>
    /// �w��[���̐�������
    /// </summary>
    /// <param name="id"></param>
    public void OnTerminalsSuccessed(int id)
    {
        terminalDatas[id-1].State = EnumManager.TERMINAL_STATE.Success;
        terminalObjs[id].GetComponent<TerminalBase>().SuccessTerminal();
    }

    /// <summary>
    /// �w��[���̎��s����
    /// </summary>
    /// <param name="id"></param>
    public void OnTerminalFailured(int id)
    {
        terminalDatas[id - 1].State = EnumManager.TERMINAL_STATE.Failure;
        terminalObjs[id].GetComponent<TerminalBase>().FailureTerminal();
    }

    /// <summary>
    /// �^�[�~�i���f�[�^�X�V
    /// </summary>
    /// <param name=""></param>
    public void OnUpdateTerminal(List<TerminalData> terminalDatas)
    {
        // �f�[�^�X�V
        TerminalDatas = terminalDatas;

        // �A�N�e�B�u���G�l�~�[���G���[�g�̏ꍇ�A�e�L�X�g�\���X�V
        for (int i = 0; i < TerminalDatas.Count; i++) 
        {
            if (TerminalDatas[i].State == EnumManager.TERMINAL_STATE.Active && TerminalDatas[i].Type == EnumManager.TERMINAL_TYPE.Enemy ||
                TerminalDatas[i].State == EnumManager.TERMINAL_STATE.Active && TerminalDatas[i].Type == EnumManager.TERMINAL_TYPE.Elite)
            {
                TerminalObjs[i].GetComponent<TerminalBase>().OnCountDown(TerminalDatas[i].Time);
            }
        }
    }
}
