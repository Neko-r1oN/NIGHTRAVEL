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
        /// 現在のステージ
        /// Author:Nishiura
        /// </summary>
        public EnumManager.STAGE_TYPE NowStage { get; set; }

        /// <summary>
        /// ステージ進行リクエスト変数
        /// Author:Nishiura
        /// </summary>
        public bool isAdvanceRequest;

        /// <summary>
        /// 合計付与ダメージ
        /// Author:Nishiura
        /// </summary>
        public int totalGaveDamage = 0;

        /// <summary>
        /// 合計キル数
        /// Author:Nishiura
        /// </summary>
        public int totalKillCount = 0;

        /// <summary>
        ///  合計被弾数
        /// Author:Nishiura
        /// </summary>
        public int totalGainDamage = 0;

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
        /// ドロップレリックリスト
        /// </summary>
        public Dictionary<string, DropRelicData> dropRelicDataList { get; } = new Dictionary<string, DropRelicData>();

        /// <summary>
        /// ギミックリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<int, GimmickData> gimmickList { get; } = new Dictionary<int, GimmickData>();

        /// <summary>
        /// レリックの情報リスト
        /// </summary>
        public List<Relic> relicDataList {  get; } = new List<Relic>();

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

        /// <summary>
        /// ステータス強化選択肢リスト
        /// </summary>
        public List<EnumManager.STAT_UPGRADE_OPTION> statusOptionList { get; } = new List<EnumManager.STAT_UPGRADE_OPTION>();

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

            // 各ステータスを初期化
            characterSetData.State = playerDataList[conID].State;
            characterSetData.Status = playerDataList[conID].Status;

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
        public void SetEnemyData(int uniqueId, Enemy enemData)
        {
            EnemyData setData = new EnemyData();

            // 受け取ったデータをエネミーデータに格納
            setData.EnemyName = enemData.name;
            setData.EnemyID = uniqueId;
            setData.isBoss = enemData.isBoss;
            setData.Exp = enemData.exp;

            // 現在ステータス
            setData.State.hp = (int)enemData.hp;
            setData.State.power = (int)enemData.attack;
            setData.State.defence = (int)enemData.defence;
            setData.State.moveSpeed = (int)enemData.move_speed;

            // 最大ステータス
            setData.Status.hp = (int)enemData.hp;
            setData.Status.power = (int)enemData.attack;
            setData.Status.defence = (int)enemData.defence;
            setData.Status.moveSpeed = (int)enemData.move_speed;

            enemyDataList.Add(uniqueId, setData);
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
