////////////////////////////////////////////////////////////////
///
/// サーバーからクライアントへの通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using MagicOnion;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using NIGHTRAVEL.Shared.Unity;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using System.Collections.Concurrent;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Shared.Interfaces.StreamingHubs
{
    public interface IRoomHubReceiver
    {
        //ここにサーバー～クライアントの定義

        #region 入室からゲーム開始まで

        /// <summary>
        /// ルーム作成通知
        /// Author:Kida
        /// </summary>
        void OnRoom();

        /// <summary>
        /// 参加失敗の通知
        /// Author:Kida
        /// </summary>
        void OnFailedJoin(int errorId);

        /// <summary>
        /// ユーザーの入室通知
        /// Author:Kida
        /// </summary>
        /// <param name="joindUserList">参加者リスト</param>
        void Onjoin(JoinedUser joindUser);

        /// <summary>
        /// ユーザーの退室通知
        /// Author:Kida
        /// </summary>
        /// <param name="user">対象者</param>
        //void OnLeave(JoinedUser user);

        void OnLeave(Dictionary<Guid,JoinedUser> user,Guid targetUser);


        /// <summary>
        /// キャラクター変更通知
        /// Author:Kida
        /// </summary>
        void OnChangeCharacter(Guid guid , int characterId);

        /// <summary>
        /// 準備完了通知
        /// </summary>
        /// <param name="conID">接続ID</param>
        void OnReady(JoinedUser joinedUser);

        /// <summary>
        /// ゲーム開始通知
        /// Author:Nishiura
        /// </summary>
        void OnStartGame();

        #endregion

        #region ゲーム内
        #region プレイヤー関連
        /// <summary>
        /// プレイヤー動作通知
        /// Author:Nishiura
        /// </summary>
        void OnUpdatePlayer(PlayerData playerData);

        /// <summary>
        /// マスタークライアントの更新通知
        /// Author:木田晃輔
        /// </summary>
        /// <param name="masterClientData"></param>
        void OnUpdateMasterClient(MasterClientData masterClientData);

        /// <summary>
        /// プレイヤー死亡通知
        /// Author:Nishiura
        /// </summary>
        /// <param name=")">プレイヤーID</param>
        void OnPlayerDead(Guid conID);

        /// <summary>
        /// 発射物の生成通知
        /// </summary>
        /// <param name="type">発射物の種類</param>
        /// <param name="spawnPos">生成位置</param>
        /// <param name="shootVec">発射ベクトル</param>
        void OnShootBullets(params ShootBulletData[] shootBulletDatas);

        /// <summary>
        /// ステータス更新通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="characterStatus"></param>
        /// <param name="prsData"></param>
        void OnUpdateStatus(CharacterStatusData characterStatus, PlayerRelicStatusData prsData);

        /// <summary>
        /// ビームエフェクトのアクティブ通知
        /// </summary>
        /// <param name="conID"></param>
        /// <param name="isActive"></param>
        void OnBeamEffectActive(Guid conID, bool isActive);

        #endregion
        #region 敵関連

        /// <summary>
        /// 敵生成通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemyData">敵情報</param>
        /// <param name="pos">敵のスポーン位置</param>
        void OnSpawnEnemy(List<SpawnEnemyData> spawnEnemyDatas);

        /// <summary>
        /// 敵体力増減通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemDmgData">敵被弾データ</param>
        void OnEnemyHealth(EnemyDamegeData enemDmgData);

        /// <summary>
        /// 指定した敵の情報削除通知
        /// </summary>
        /// <param name="enemId"></param>
        void OnDeleteEnemy(string enemId);

        #endregion
        #region レリック関連

        /// <summary>
        /// レリック生成通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="relicDatas">レリックリスト</param>
        void OnDropRelic(Dictionary<string, DropRelicData>relicDatas);
        #endregion
        #region 端末関連
        /// <summary>
        /// 端末起動通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="termID">端末識別ID</param>
        void OnBootTerminal(int termID);

        /// <summary>
        /// ジャンブル結果通知
        /// </summary>
        /// <param name="relics"></param>
        void OnTerminalJumble(List<DropRelicData> relics);

        /// <summary>
        /// 端末結果通知
        /// </summary>
        /// <param name="termID">端末識別ID</param>
        /// <param name="result">端末結果</param>
        void OnTerminalsSuccess(int termID);

        /// <summary>
        /// 端末失敗通知
        /// </summary>
        /// <param name="termID"></param>
        void OnTerminalFailure(int termID);
        #endregion
        #region ゲーム内UI、仕様

        /// <summary>
        /// 同時開始通知
        /// Author:木田晃輔
        /// </summary>
        void OnSameStart(List<TerminalData> list);

        /// <summary>
        /// ギミック起動通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="gimID">ギミックID</param>
        void OnBootGimmick(string uniqueID, bool triggerOnce);

        /// <summary>
        /// 難易度上昇通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="dif">増加後難易度</param>
        void OnAscendDifficulty(int dif);

        /// <summary>
        /// 次ステージ進行通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">接続ID</param>
        /// <param name="isAdvance">次ステージ進行判定</param>
        /// <param name="stageType">次ステージ</param>
        void OnAdanceNextStage(STAGE_TYPE stageType);

        /// <summary>
        /// オブジェクト生成通知
        /// </summary>
        /// <returns></returns>
        void OnSpawnObject(OBJECT_TYPE type, Vector2 spawnPos, string uniqueId);

        #endregion
        #endregion

        /// <summary>
        /// マスタークライアントの変更通知
        /// </summary>
        void OnChangeMasterClient();

        void OnGameEnd(ResultData result);

        /// <summary>
        /// アイテム獲得通知
        /// </summary>
        /// <param name="conId">獲得したユーザーの接続ID</param>
        /// <param name="itemID">アイテムの識別用ID</param>
        /// <param name="nowLevel">現在の経験値</param>
        /// <param name="nowExp">現在のEXP</param>
        /// <param name="nextLevelExp">レベルアップに必要なEXP</param>
        void OnGetItem(Guid conId, string itemID, int nowLevel, int nowExp, int nextLevelExp);

        #region 不要になりそうなAPI

        #region ゲーム内
        #region プレイヤー関連

        /// <summary>
        /// レベルアップ通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="level">レベル</param>
        /// <param name="nowExp">現在の経験値</param>
        /// <param name="characterStatusDataList">プレイヤーステータスリスト</param>
        /// <param name="statusOptionList">強化選択肢リスト</param>
        void OnLevelUp(int level, int nowExp, int nextExp,CharacterStatusData updatedStatusData, Guid optionsKey, List<StatusUpgrateOptionData> statusOptionList);

        #endregion
        #endregion

        #endregion
    }
}
