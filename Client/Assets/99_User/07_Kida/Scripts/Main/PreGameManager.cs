using Shared.Interfaces.StreamingHubs;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.IRoomHubReceiver;

public class PreGameManager : MonoBehaviour
{
    [SerializeField] RoomModel roomModel;           //RoomModelクラスを使用
    [SerializeField] GameObject player;
    [SerializeField] GameObject enemy;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //ユーザーが移動した時にOnMoveUserメソッドを実行するよう、モデルに登録
        roomModel.OnMovePlayerSyn += this.OnMoveCharacterSyn;
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
        
    }

    /// <summary>
    /// プレイヤーの移動通知
    /// </summary>
    public void OnMoveCharacterSyn(JoinedUser user, Vector2 pos, Quaternion rot, CharacterState animID)
    {

    }

    /// <summary>
    /// 敵の移動同期
    /// </summary>
    void EnemyUpdate()
    {

    }

}
