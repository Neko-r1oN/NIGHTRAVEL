//--------------------------------------------------------------
// ターミナル管理クラス [ TerminalManager.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;

public class TerminalManager : MonoBehaviour
{
    //--------------------------------
    // フィールド

    #region 外部情報設定

    /// <summary>
    /// 端末生成位置
    /// </summary>
    [SerializeField] private GameObject[] generatePos;

    /// <summary>
    /// 生成端末プレハブ
    /// </summary>
    [SerializeField] private GameObject[] terminalPrefabs;

    #endregion

    #region 端末情報

    /// <summary>
    /// 生成端末リスト
    /// </summary>
    private List<TerminalData> terminalDatas = new List<TerminalData>();

    /// <summary>
    /// 生成された端末オブジェクトリスト
    /// </summary>
    private Dictionary<int, GameObject> terminalObjs = new Dictionary<int, GameObject>();

    #endregion

    #region 情報参照用プロパティ

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
    // メソッド

    /// <summary>
    /// 起動処理
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            // インスタンスが複数存在しないように、既に存在していたら自身を消去する
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初期処理
    /// </summary>
    private async void Start()
    {
        if (RoomModel.Instance)
        {
            RoomModel.Instance.OnBootedTerminal += this.OnBootedTerminal;
            await RoomModel.Instance.AdvancedStageAsync();  //遷移完了のリクエスト
        }
    }

    /// <summary>
    /// 端末の生成処理
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
    /// ターミナル情報の更新
    /// </summary>
    /// <param name="gimmicks"></param>
    public void UpdateTerminals(List<TerminalData> terminals)
    {
        terminalDatas = terminals;
    }

    /// <summary>
    /// 指定端末の起動処理
    /// </summary>
    /// <param name="ID"></param>
    public void OnBootedTerminal(int ID)
    {
        terminalDatas[ID - 1].State = EnumManager.TERMINAL_STATE.Active;
        terminalObjs[ID - 1].GetComponent<TerminalBase>().BootTerminal();
    }
}
