//=============================
// ルームコンテキストスクリプト
// Author:木田晃輔
//=============================
using Cysharp.Runtime.Multicast;
using NIGHTRAVEL.Server.StreamingHubs;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Numerics;
using UnityEngine;

namespace NIGHTRAVEL.Server.Model.Context
{
    public class RoomContext
    {
        #region RoomContext基本構造
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
        /// 難易度
        /// Author:Nishiura
        /// </summary>
        public int NowDifficulty { get; set; }

        /// <summary>
        /// 経験値管理クラス
        /// Author:Nishiura
        /// </summary>
        public ExpManager ExpManager { get; set; }

        /// <summary>
        /// グループ
        /// Author:Kida
        /// </summary>
        public IMulticastSyncGroup<Guid, IRoomHubReceiver> Group { get; }
        #endregion

        #region コンテキストに保存する情報のリスト一覧
        /// <summary>
        /// 参加者リスト
        /// Author:Kida
        /// </summary>
        public Dictionary<Guid, JoinedUser> JoinedUserList { get; } = new Dictionary<Guid, JoinedUser>();

        /// <summary>
        /// ルームデータリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<Guid, PlayerData> playerDataList { get; } = new Dictionary<Guid, PlayerData>();

        /// <summary>
        /// キャラクターデータリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<Guid, CharacterData> characterDataList = new Dictionary<Guid, CharacterData>();

        /// <summary>
        /// エネミーデータリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<int, EnemyData> enemyDataList { get; } = new Dictionary<int, EnemyData>();

        /// <summary>
        /// ギミックデータリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<int, GimmickData> gimmickDataList { get; } = new Dictionary<int, GimmickData>();

        /// <summary>
        /// 生成された敵リスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<int, SpawnEnemyData> spawnedEnemyDataList { get; } = new Dictionary<int, SpawnEnemyData>();

        /// <summary>
        /// ドロップレリックリスト
        /// </summary>
        public List<DropRelicData> dropRelicDataList { get; } = new List<DropRelicData>();

        /// <summary>
        /// 起動済み端末IDリスト
        /// Author:Nishiura
        /// </summary>
        public List<int> bootedTerminalList { get; } = new List<int>();

        /// <summary>
        /// 端末結果リスト
        /// Author:Nishiura
        /// </summary>
        public List<int> succededTerminalList { get; } = new List<int>();

        /// <summary>
        /// 取得アイテムリスト
        /// Author:Nishiura
        /// </summary>
        public List<string> gottenItemList { get; } = new List<string>();

        //[その他、ゲームのルームデータをフィールドに保存]
        #endregion

        //RoomContextの定義
        public RoomContext(IMulticastGroupProvider groupProvider, string roomName)
        {
            Id = Guid.NewGuid();
            Name = roomName;
            Group =
                groupProvider.GetOrAddSynchronousGroup<Guid, IRoomHubReceiver>(roomName);
        }

        #region 独自関数
        /// <summary>
        /// データ追加処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">自身の接続ID</param>
        public void AddPlayerData(Guid conID)
        {
            PlayerData playerSetData = new PlayerData();

            //初期データを設定
            //playerSetData.JoinedUser = new JoinedUser();
            //playerSetData.PlayerID = 0;
            //playerSetData.Health = 0;
            //playerSetData.Attack = 0;
            //playerSetData.AttackSpeed = 0;
            //playerSetData.Defense = 0;
            //playerSetData.Speed = 0;
            //playerSetData.Position = new UnityEngine.Vector2(0, 0);
            //playerSetData.Rotation = new UnityEngine.Quaternion(0, 0, 0, 0);
            //playerSetData.State = 0;
            //playerSetData.DebuffList = new List<int> { 0, 0, 0 };
            //playerSetData.IsDead = false;

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

        /// <summary>
        /// キャラクターデータ追加処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID"></param>
        public void AddCharacterData(Guid conID)
        {
            CharacterData characterSetData = new CharacterData();

            characterDataList.Add(conID, characterSetData);
        }

        /// <summary>
        /// キャラクターデータ更新処理
        /// </summary>
        /// <param name="conID"></param>
        /// <param name="charaData"></param>
        public void UpdateCharacterData(Guid conID, CharacterData charaData)
        {
            characterDataList[conID] = charaData;
        }

        /// <summary>
        /// グループ退室処理
        /// Author:木田晃輔
        /// </summary>
        public void Dispose()
        {
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

        /// <summary>
        /// 敵の情報を渡す関数
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemData"></param>
        public void SetEnemyData(Enemy enemData)
        {
            EnemyData setData = new EnemyData();

            // 受け取ったデータをエネミーデータに格納
            setData.EnemyName = enemData.name;
            //setData.Health = (int)enemData.hp;
            //setData.AttackPower = (int)enemData.attack;
            //setData.Defense = (int)enemData.defence;
            //setData.MoveSpeed = (float)enemData.move_speed;
            //setData.AttackSpeedFactor = (float)enemData.attack_speed;
            setData.Exp = enemData.exp;

            enemyDataList.Add(enemData.id, setData);
        }

        /// <summary>
        /// 生成された敵の情報を入力する関数
        /// Author:Nishiura
        /// </summary>
        public void  SetSpawnedEnemyData(List<SpawnEnemyData> enemyList)
        {
            // 渡された敵のリスト分ループしてデータを追加
            for (int i = 0; i < enemyList.Count; i++)
            {
                spawnedEnemyDataList.Add(enemyList[i].EnemyId, enemyList[i]);
            }
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
        #endregion
    }
}
