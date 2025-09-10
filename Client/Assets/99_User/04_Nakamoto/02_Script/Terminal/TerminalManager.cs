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

    /// <summary>
    /// �����[�����X�g
    /// </summary>
    private List<TerminalData> terminalDatas = new List<TerminalData>();

    /// <summary>
    /// �������ꂽ�[���I�u�W�F�N�g���X�g
    /// </summary>
    private Dictionary<int,GameObject> terminalObjs = new Dictionary<int, GameObject>();

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
    /// �[���̐�������
    /// </summary>
    /// <param name="list"></param>
    public void SetTerminal(List<TerminalData> list)
    {
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
    public void BootTerminal(int ID)
    {
        terminalObjs[ID].GetComponent<TerminalBase>().BootTerminal();
    }
}
