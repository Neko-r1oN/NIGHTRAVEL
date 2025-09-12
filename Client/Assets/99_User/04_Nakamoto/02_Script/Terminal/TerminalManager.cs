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

        foreach (var data in list)
        {
            var terminal = Instantiate(terminalPrefabs[(int)data.Type - 1], generatePos[data.ID - 1].transform.position, Quaternion.identity);
            terminal.GetComponent<TerminalBase>().TerminalID = data.ID;
            terminal.GetComponent<TerminalBase>().TerminalType = data.Type;

            terminalObjs.Add(data.ID, terminal);
        }
    }

    /// <summary>
    /// �^�[�~�i�����̍X�V
    /// </summary>
    /// <param name="gimmicks"></param>
    public void UpdateTerminals(List<TerminalData> terminals)
    {
        terminalDatas = terminals;
    }

    /// <summary>
    /// �w��[���̋N������
    /// </summary>
    /// <param name="ID"></param>
    public void OnBootedTerminal(int ID)
    {
        terminalDatas[ID - 1].State = EnumManager.TERMINAL_STATE.Active;
        terminalObjs[ID - 1].GetComponent<TerminalBase>().BootTerminal();
    }
}
