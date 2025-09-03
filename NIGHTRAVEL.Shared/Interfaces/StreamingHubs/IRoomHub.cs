//=============================
// クライアントからサーバーへの通信を管理するスクリプト
// Author:木田晃輔
//=============================

using MagicOnion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.IRoomHubReceiver;

namespace Shared.Interfaces.StreamingHubs
{
    public interface IRoomHub:IStreamingHub<IRoomHub,IRoomHubReceiver>
    {
        //ここにクライアント～サーバー定義

        #region 入室からゲーム開始まで
        /// <summary>
        /// ユーザー入室
        /// Author:Kida
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Dictionary<Guid, JoinedUser>> JoinedAsync(string roomName, int userId);

        /// <summary>
        /// ユーザー退室
        /// Author:Kida
        /// </summary>
        /// <returns></returns>
        Task LeavedAsync();

        /// <summary>
        /// 準備完了
        /// Author:Nishiura
        /// </summary>
        /// <returns></returns>
        Task ReadyAsync();

        #endregion

        #region ゲーム内
        #region プレイヤー関連

        /// <summary>
        /// プレイヤーの更新
        /// </summary>
        /// <param name="playerData"></param>
        /// <returns></returns>
        Task UpdatePlayerAsync(PlayerData playerData);

        /// <summary>
        /// マスタークライアントの更新
        /// Author:木田晃輔
        /// </summary>
        /// <param name="masterClientData"></param>
        /// <returns></returns>
        Task UpdateMasterClientAsync(MasterClientData masterClientData);

        /// <summary>
        /// プレイヤー死亡
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">プレイヤーID</param>
        /// <returns></returns>
        Task PlayerDeadAsync();

        /// <summary>
        /// 端末起動
        /// Author:Nishiura
        /// </summary>
        /// <param name="termID">端末種別ID</param>
        /// <returns></returns>
        Task BootTerminalAsync(int termID);

        /// <summary>
        /// 端末成功処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="termID">端末種別ID</param>
        /// <param name="result">端末結果</param>
        /// <returns></returns>
        Task TerminalsResultAsync(int termID, bool result);

        /// <summary>
        /// アイテム獲得
        /// Author:Nishiura
        /// </summary>
        /// <param name="itemType">アイテムの種類</param>
        /// <param name="itemID">識別ID(文字列)</param>
        /// <returns></returns>
        Task GetItemAsync(EnumManager.ITEM_TYPE itemType, string itemID);

        /// <summary>
        /// 弾発射
        /// Author:Nishiura
        /// </summary>
        /// <param name="spawnPos">生成位置</param>
        /// <param name="shootVec">発射ベクトル</param>
        /// <returns></returns>
        Task ShootBulletAsync(Vector2 spawnPos, Vector2 shootVec);
        #endregion
        #region 敵関連
        /// <summary>
        /// 敵生成
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemyData">敵の情報</param>
        /// <param name="pos">敵の生成位置</param>
        /// <returns></returns>
        Task SpawnEnemyAsync(List<SpawnEnemyData> spawnEnemyDatas);

        /// <summary>
        /// 敵体力増減
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <param name="giverATK">PLの攻撃力</param>
        /// <param name="debuffType">デバフの種類</param>
        /// <returns></returns>
        Task EnemyHealthAsync(int enemID, float giverATK, List<EnumManager.DEBUFF_TYPE> debuffType);

        /// <summary>
        /// 敵の被ダメージ同期処理   プレイヤーによるダメージ以外
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <param name="dmgAmount">適用させるダメージ量</param>
        /// <returns></returns>
        Task ApplyDamageToEnemyAsync(int enemID, int dmgAmount);

        /// <summary>
        /// ステータス強化選択
        /// </summary>
        /// <param name="conID">接続ID</param>
        /// <param name="upgradeOpt">強化項目</param>
        /// <returns></returns>
        Task<CharacterStatusData> ChooseUpgrade(EnumManager.STAT_UPGRADE_OPTION upgradeOpt);
        #endregion
        #region レリック関連

        /// <summary>
        /// レリックの情報を取得
        /// Author:木田晃輔
        /// </summary>
        /// <returns></returns>
        Task GetRelicIntelligenceAsync();

        /// <summary>
        /// レリック位置
        /// Author:Nishiura
        /// </summary>
        /// <param name="pos">位置</param>
        /// <returns></returns>
        Task DropRelicAsync(Stack<Vector2> pos);

        /// <summary>
        /// レリック取得
        /// Author:Nishiura
        /// </summary>
		/// <param name="relicID">レリックID</param>
        /// <param name="relicName">レリック名</param>
        /// <returns></returns>
        Task GetRelicAsync(int relicID, string relicName);
        #endregion
        #region ゲーム内UI、仕様関連

        /// <summary>
        /// 同時開始処理
        /// Author:木田晃輔
        /// </summary>
        /// <returns></returns>
        Task SameStartAsync();

        /// <summary>
        /// ギミック起動
        /// Author:Nishiura
        /// </summary>
        /// <param name="gimID">ギミック識別ID</param>
        /// <returns></returns>
        Task BootGimmickAsync(int gimID);

        /// <summary>
        /// 難易度上昇
        /// Author:Nishiura
        /// </summary>
        /// <param name="difID">難易度ID</param>
        /// <returns></returns>
        Task AscendDifficultyAsync();

        /// <summary>
        /// ステージクリア
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">接続ID</param>
        /// <param name="isAdvance">ステージ進行判定</param>
        /// <returns></returns>
        Task StageClear(bool isAdvance);

        /// <summary>
        /// ステージ進行完了
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">接続ID</param>
        /// <param name="isAdvance">ステージ進行判定</param>
        /// <returns></returns>
        Task AdvancedStageAsync(bool isAdvance);

        #endregion
        #endregion

        /// <summary>
        /// マスタークライアントが退室したときの処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">接続ID</param>
        /// <returns></returns>
        Task MasterLostAsync(Guid conID);

        /// <summary>
        /// 時間同期処理
        /// </summary>
        /// <param name="time">タイマーの辞典</param>
        /// <returns></returns>
        Task TimeAsync(Dictionary<EnumManager.TIME_TYPE, int> tiemrType);

        /// <summary>
        /// リザルト
        /// Author:Nishiura
        /// </summary>
        /// <returns></returns>
        Task Result();

        #region 不要になりそうなAPI
        #region ゲーム内

        /// <summary>
        /// ステージクリア待機
        /// </summary>
        /// <param name="conID">接続ID</param>
        /// <param name="isTimeUp">時間切れ判定</param>
        /// <param name="isAdvance">ステージ進行判定</param>
        /// <returns></returns>
        Task WaitStageClearAsync(Guid? conID, bool isTimeUp, bool isAdvance);

        #region プレイヤー関連
        /// <summary>
        /// プレイヤー体力増減
        /// Author:Nishiura
        /// </summary>
        /// <param name="playerID">プレイヤー識別ID</param>
        /// <param name="playerHP">プレイヤー体力</param>
        /// <returns></returns>
        Task PlayerHealthAsync(int playerID, float playerHP);

        /// <summary>
        /// 経験値同期
        /// Author:Nishiura
        /// </summary>
        /// <param name="exp">経験値</param>
        /// <returns></returns>
        Task EXPAsync(int exp);

        /// <summary>
        /// レベルアップ同期
        /// Author:Nishiura
        /// </summary>
        /// <returns></returns>
        Task LevelUpAsync();

        #endregion
        #region 敵関連

        /// <summary>
        /// 敵死亡
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <returns></returns>
        Task KilledEnemyAsync(int enemID);
        #endregion

        #region ゲーム内UI、仕様関連

        /// <summary>
        /// ダメージ表記
        ///  Author:Nishiura
        /// </summary>
        /// <param name="dmg">ダメージ</param>
        /// <returns></returns>
        Task DamageAsync(int dmg);
        #endregion
        #endregion
        #endregion
    }
}
