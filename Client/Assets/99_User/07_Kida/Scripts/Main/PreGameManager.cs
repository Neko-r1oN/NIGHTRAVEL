using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.IRoomHubReceiver;

public class PreGameManager : MonoBehaviour
{
    [SerializeField] RoomModel roomModel;           //RoomModelクラスを使用
    [SerializeField] GameObject player;
    [SerializeField] GameObject enemy;

    //ユーザーIDとゲームオブジェクトを同時に格納
    Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();

    int anim = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //ユーザーが入室した時にOnJoinedUserメソッドを実行するよう、モデルに登録(デバッグ用)
        roomModel.OnJoinedUser += this.OnJoinedUser;
        //ユーザーが退室した時にOnLeavedUserメソッドを実行するよう、モデルに登録(デバッグ用)
        roomModel.OnLeavedUser += this.OnLeavedUser;
        //ユーザーが移動した時にOnMoveUserメソッドを実行するよう、モデルに登録
        //roomModel.OnMovePlayerSyn += this.OnMoveCharacterSyn;
        //敵が移動した時にOnMoveUserメソッドを実行するよう、モデルに登録
        roomModel.OnMoveEnemySyn += this.OnMoveEnemySyn;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// プレイヤーの移動同期
    /// </summary>
    public async　void PlayerUpdate()
    {
        //await roomModel.MovePlayerAsync(playerData);
    }

    /// <summary>
    /// 敵の移動同期
    /// </summary>
    void EnemyUpdate()
    {

    }

    /// <summary>
    /// 入室完了通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {
        //入室したときの処理を書く
        Debug.Log(joinedUser.UserData.Name + "が入室しました。");

        //自分であればプレイヤーを出現させる
        if(roomModel.ConnectionId==joinedUser.ConnectionId)
        {
            Instantiate(player);
        }
    }

    /// <summary>
    /// 退室完了通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnLeavedUser(JoinedUser joinedUser)
    {
        //退室したときの処理を書く
        Debug.Log(joinedUser.UserData.Name + "が退室しました。");

        //退室したプレイヤーを消す
        player.SetActive(false);
    }

    /// <summary>
    /// プレイヤーの移動通知
    /// </summary>
    public void OnMoveCharacterSyn(JoinedUser user, Vector2 pos, Quaternion rot, CharacterState animID)
    {

    }

    /// <summary>
    /// 敵の移動通知
    /// </summary>
    public void OnMoveEnemySyn(int enemID, Vector2 pos, Quaternion rot, EnemyAnimState anim)
    {

    }

}
