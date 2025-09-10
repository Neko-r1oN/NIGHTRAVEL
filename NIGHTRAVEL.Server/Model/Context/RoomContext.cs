//=============================
// ルームコンテキストスクリプト
// Author:木田晃輔
//=============================
using Cysharp.Runtime.Multicast;
using NIGHTRAVEL.Server.StreamingHubs;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
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
        public ExpManager ExpManager { get; set; } = new ExpManager();

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
        /// キャラクターデータリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<Guid, PlayerData> characterDataList = new Dictionary<Guid, PlayerData>();

        /// <summary>
        /// プレイヤー毎の最大ステータスリスト
        /// </summary>
        public Dictionary<Guid, (CharacterStatusData, PlayerRelicStatusData)> playerStatusDataList = new Dictionary<Guid, (CharacterStatusData, PlayerRelicStatusData)>();

        /// <summary>
        /// エネミーデータリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<string, EnemyData> enemyDataList { get; } = new Dictionary<string, EnemyData>();

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
        /// 端末情報リスト
        /// </summary>
        public List<TerminalData> terminalList { get; set; } = new List<TerminalData>();

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
        /// キャラクターデータ追加処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID"></param>
        public void AddCharacterData(Guid conID, PlayerData playerData)
        {
            characterDataList.Add(conID, playerData);
            playerStatusDataList.Add(conID, (playerData.Status, new PlayerRelicStatusData()));
        }

        /// <summary>
        /// キャラクターデータ削除
        /// </summary>
        public void RemoveCharacterData(Guid conID)
        {
            characterDataList.Remove(conID);
        }

        /// <summary>
        /// キャラクターデータ更新処理
        /// </summary>
        /// <param name="conID"></param>
        /// <param name="charaData"></param>
        public void UpdateCharacterData(Guid conID, PlayerData charaData)
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
            return characterDataList[conID];
        }

        /// <summary>
        /// 敵情報を渡す関数
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID"></param>
        public EnemyData GetEnemyData(string uniqueId)
        {
            return enemyDataList[uniqueId];
        }

        /// <summary>
        /// 敵の情報を渡す関数
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemData"></param>
        public void SetEnemyData(string uniqueId, Enemy enemData)
        {
            EnemyData setData = new EnemyData();

            // 受け取ったデータをエネミーデータに格納
            setData.EnemyName = enemData.name;
            setData.UniqueId = uniqueId;
            setData.isBoss = enemData.isBoss;
            setData.Exp = enemData.exp;

            // 最大ステータスを現在のステータスに設定
            setData.Status.hp = (int)enemData.hp;
            setData.Status.power = (int)enemData.power;
            setData.Status.defence = (int)enemData.defence;
            setData.Status.jumpPower = (int)enemData.jump_power;
            setData.Status.moveSpeed = (int)enemData.move_speed;
            setData.Status.attackSpeedFactor = (int)enemData.attack_speed_factor;
            setData.State = setData.Status;

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
