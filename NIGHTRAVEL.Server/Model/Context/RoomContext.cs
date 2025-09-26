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
using System.Diagnostics;
using System.Numerics;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

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
        /// パスワード
        /// Author:Kida
        /// </summary>
        public string PassWord { get; set; }

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
        /// 合計クリアステージ数
        /// Author:Nishiura
        /// </summary>
        public int totalClearStageCount = 0;

        /// <summary>
        /// ゲーム開始時の時刻
        /// </summary>
        public DateTime startTime;

        /// <summary>
        /// グループ
        /// Author:Kida
        /// </summary>
        public IMulticastSyncGroup<Guid, IRoomHubReceiver> Group { get; }

        /// <summary>
        /// ゲームスタート
        /// </summary>
        public bool IsStartGame { get; set; } = false;

        #region マスタデータ

        /// <summary>
        /// マスタデータを読み込み済みかどうか
        /// </summary>
        public bool IsLoadMasterDatas { get; private set; } = false;

        /// <summary>
        /// 敵のマスタデータ
        /// </summary>
        public Dictionary<EnumManager.ENEMY_TYPE, Enemy> enemyMasterDataList { get; set; } = new Dictionary<EnumManager.ENEMY_TYPE, Enemy>();

        /// <summary>
        /// ステータス強化の選択肢の種類
        /// </summary>
        public Dictionary<EnumManager.STAT_UPGRADE_OPTION, Status_Enhancement> statusOptionMasterDataList { get; set; } = new Dictionary<EnumManager.STAT_UPGRADE_OPTION, Status_Enhancement>();

        #endregion

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
        public Dictionary<string, GimmickData> gimmickList { get; set; } = new Dictionary<string, GimmickData>();

        /// <summary>
        /// レリックの情報リスト
        /// </summary>
        public Dictionary<Guid,List<Relic>> relicDataList { get; } = new Dictionary<Guid, List<Relic>>();

        /// <summary>
        /// リザルトデータリスト
        /// </summary>
        public Dictionary<Guid,ResultData> resultDataList { get; }= new Dictionary<Guid, ResultData>();

        /// <summary>
        /// 端末情報リスト
        /// </summary>
        public List<TerminalData> terminalList { get; } = new List<TerminalData>();

        /// <summary>
        /// 取得アイテムリスト
        /// Author:Nishiura
        /// </summary>
        public List<string> gottenItemList { get; } = new List<string>();

        /// <summary>
        /// ステータス強化選択肢リスト
        /// key1:ユーザー毎のconnectionId
        /// key2:ユニークid
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, List<StatusUpgrateOptionData>>> statusOptionList { get; set; } = new Dictionary<Guid, Dictionary<Guid, List<StatusUpgrateOptionData>>>();

        #endregion

        //RoomContextの定義
        public RoomContext(IMulticastGroupProvider groupProvider, string roomName , string pass)
        {
            Id = Guid.NewGuid();
            Name = roomName;
            PassWord = pass;
            Group =
                groupProvider.GetOrAddSynchronousGroup<Guid, IRoomHubReceiver>(roomName);
        }

        #region 独自関数

        /// <summary>
        /// 複数のマスタデータをロードする
        /// </summary>
        /// <param name="dbContext"></param>
        public void LoadMasterDataLists(GameDbContext dbContext)
        {
            IsLoadMasterDatas = true;
            foreach (var item in dbContext.Enemies.ToList())
            {
                enemyMasterDataList.Add((ENEMY_TYPE)item.id, item);
            }

            foreach (var item in dbContext.Status_Enhancements.ToList())
            {
                statusOptionMasterDataList.Add((STAT_UPGRADE_OPTION)item.id, item);
            }
        }

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
        /// グループ退室処理
        /// Author:木田晃輔
        /// </summary>
        public void Dispose()
        {
            Group.Dispose();
        }

        /// <summary>
        /// 敵の情報を渡す関数
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemData"></param>
        public void SetEnemyData(string uniqueId, Enemy enemData, EnumManager.ENEMY_TYPE type)
        {
            EnemyData setData = new EnemyData();

            // 受け取ったデータをエネミーデータに格納
            setData.EnemyName = enemData.name;
            setData.UniqueId = uniqueId;
            setData.TypeId = type;
            setData.isBoss = enemData.isBoss;

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

        /// <summary>
        /// ユーザー毎でステータス強化の選択肢を格納する
        /// </summary>
        /// <param name="connectionId">ユーザーの接続ID</param>
        /// <param name="optioinsKey">選択肢リストのグループkey</param>
        /// <param name="options">選択肢リスト</param>
        public void AddStatusOptions(Guid connectionId, Guid optioinsKey, List<StatusUpgrateOptionData> options)
        {
            // 外側キーが存在しない場合は作成
            if (!statusOptionList.ContainsKey(connectionId))
            {
                statusOptionList[connectionId] = new Dictionary<Guid, List<StatusUpgrateOptionData>>();
            }
            // 内側キーが存在しない場合は作成
            if (!statusOptionList[connectionId].ContainsKey(optioinsKey))
            {
                statusOptionList[connectionId][optioinsKey] = new List<StatusUpgrateOptionData>();
            }
            statusOptionList[connectionId][optioinsKey].AddRange(options);
        }
        #endregion
    }
}
