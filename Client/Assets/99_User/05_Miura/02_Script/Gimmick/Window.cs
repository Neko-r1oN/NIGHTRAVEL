//====================
// 送風ファンのスクリプト
// Aouther:y-miura
// Date:2025/07/08
//====================

using System.Collections;
using UnityEngine;

public class Window : GimmickBase
{
    [SerializeField] GameObject windObj;
    bool isWind;

    // マスタクライアントが自身に切り替わったとき用
    const float repeatRate = 5f;
    float timer = 0;

    void Start()
    {
        // オフライン時 or マルチプレイ時に自身がマスタクライアントの場合
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
    /// 風を流したり止めたりする処理
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
    /// ギミック起動リクエスト
    /// </summary>
    void RequestActivateGimmick()
    {
        TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
    }

    /// <summary>
    /// ギミック起動処理
    /// </summary>
    public override void TurnOnPower()
    {
        SendWind();
    }

    /// <summary>
    /// ギミック再起動処理
    /// </summary>
    public override void Reactivate()
    {
        if(timer > repeatRate) timer = repeatRate;
        InvokeRepeating("RequestActivateGimmick", repeatRate - timer, repeatRate);
    }
}
