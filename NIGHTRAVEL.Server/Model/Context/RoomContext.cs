//=============================
// ルームコンテキストスクリプト
// Author:木田晃輔
//=============================
using Cysharp.Runtime.Multicast;
using NIGHTRAVEL.Server.StreamingHubs;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using System.Numerics;
using UnityEngine;

namespace NIGHTRAVEL.Server.Model.Context
{
    public class RoomContext
    {
        /// <summary>
        /// コンテキストID
        /// Author:Kida
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// ルーム名
        /// Author:Kida
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// グループ
        /// Author:Kida
        /// </summary>
        public IMulticastSyncGroup<Guid, IRoomHubReceiver> Group { get; }

        /// <summary>
        /// 参加者リスト
        /// Author:Kida
        /// </summary>
        public Dictionary<Guid, JoinedUser> JoinedUserList { get; } = new();

        /// <summary>
        /// ルームデータリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<Guid, PlayerData> playerDataList { get; } = new();

        /// <summary>
        /// エネミーデータリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<int, EnemyData> enemyDataList { get; } = new();

        /// <summary>
        /// ギミックデータリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<int, GimmickData> gimmickDataList { get; } = new();

        //[その他、ゲームのルームデータをフィールドに保存]

        public RoomContext(IMulticastGroupProvider groupProvider, string roomName)
        {
            Id = Guid.NewGuid();
            Name = roomName;
            Group =
                groupProvider.GetOrAddSynchronousGroup<Guid, IRoomHubReceiver>(roomName);
        }

        /// <summary>
        /// データ追加処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">自身の接続ID</param>
        public void AddPlayerData(Guid conID)
        {
            PlayerData playerSetData = new PlayerData();

            //初期データを設定
            playerSetData.JoinedUser = new JoinedUser();
            playerSetData.PlayerID = 0;
            playerSetData.Health = 0;
            playerSetData.Attack = 0;
            playerSetData.AttackSpeed = 0;
            playerSetData.Defense = 0;
            playerSetData.Speed = 0;
            playerSetData.Position = new UnityEngine.Vector2(0, 0);
            playerSetData.Rotation = new UnityEngine.Quaternion(0, 0, 0, 0);
            playerSetData.State = 0;
            playerSetData.DebuffList = new List<int> { 0, 0, 0 };
            playerSetData.IsDead = false;

            // データリストに自身のデータを追加
            playerDataList.Add(conID, playerSetData);
        }

        /// <summary>
        /// データ削除処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">自身の接続ID</param>
        public void RemovePlayerData(Guid conID)
        {
            // データリストから自身のデータを消去
            playerDataList.Remove(conID);
        }

        public void Dispose() {
            Group.Dispose();
        }

        /// <summary>
        /// ユーザ情報を渡す関数
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID"></param>
        public PlayerData GetPlayerData(Guid conID)
        {
            return playerDataList[conID];
        }

        /// <summary>
        /// 敵情報を渡す関数
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID"></param>
        public EnemyData GetEnemyData(int enemID)
        {
            return enemyDataList[enemID];
        }

        public void SetEnemyData(Enemy enemData)
        {
            EnemyData setData = new EnemyData();

            // 受け取ったデータをエネミーデータに格納
            setData.EnemyName = enemData.name;
            setData.Health = enemData.hp;
            setData.Attack = enemData.attack;
            setData.Defense = enemData.defence;
            setData.Speed = enemData.move_speed;
            setData.AttackSpeed = enemData.attack_speed;

            enemyDataList.Add(enemData.id, setData);
        }

        /// <summary>
        /// ギミック情報を渡す関数
        /// Author:Nishiura
        /// </summary>
        /// <param name="gimID"></param>
        /// <returns></returns>
        public GimmickData GetGimmickData(int gimID)
        {
            return gimmickDataList[gimID];
        }

        /// <summary>
        /// ユーザーの退出処理
        /// Aughter:Kida
        /// </summary>
        /// <returns></returns>
        public void RemoveUser(Guid guid)
        {
            if (JoinedUserList != null)
            { //参加者リストが存在している場合
                // 退出したユーザーを特定して削除
                JoinedUserList.Remove(guid);
            }
        }
    }
}
