using UnityEngine;

abstract public class GimmickBase : MonoBehaviour
{
    protected bool isBoot = false;    // true：ON, false：OFF
    public bool IsBoot {  get { return isBoot; } set { isBoot = value; } }

    /// <summary>
    /// ギミックの起動
    /// </summary>
    /// <param name="triggerID"></param>
    public virtual void TurnOnPower(int triggerID)
    {
        isBoot = true;
    }

    /// <summary>
    /// ギミック起動リクエスト
    /// </summary>
    /// <param name="player">起動したプレイヤー</param>
    public async void TurnOnPowerRequest(GameObject player)
    {
        // オフライン用
        if (!RoomModel.Instance)
        {
            TurnOnPower(0);
        }
        // マルチプレイ中 && 起動した人が自分自身の場合
        else if (RoomModel.Instance && player == CharacterManager.Instance.PlayerObjSelf)
        {
            // サーバーに対してリクエスト処理
        }
    }

    /// <summary>
    /// マスタクライアントによるギミック起動
    /// ※対象のギミック：SawBladeやBurnなど、一定間隔で動作するギミック
    /// </summary>
    public virtual void TuenOnPowerByMaster()
    {
        // オフライン時 || マルチプレイ中かつ自身がマスタクライアントの場合
        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            TurnOnPower(0);
        }
    }

    // 後で削除
    abstract public void TruggerRequest();

}
