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

namespace Shared.Interfaces.StreamingHubs
{
    public interface IRoomHubReceiver
    {
        //ここにサーバー～クライアントの定義

        #region 入室からゲーム開始まで
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
        void OnLeave(JoinedUser user);

        /// <summary>
        /// 準備完了通知
        /// </summary>
        /// <param name="conID">接続ID</param>
        void OnReady(Guid conID);

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
        /// <param name="user">対象者</param>
        /// <param name="pos">位置</param>
        /// <param name="rot">回転</param>
        /// <param name="animID">アニメーションID</param>
        //void OnMovePlayer(JoinedUser user, Vector2 pos, Quaternion rot, CharacterState anim);
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
        /// 端末起動通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="termID">端末識別ID</param>
        void OnBootTerminal(int termID);

        /// <summary>
        /// 端末結果通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="termID">端末識別ID</param>
        /// <param name="result">端末結果</param>
        void OnTerminalsResult(int termID, bool result);
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
        /// 敵死亡通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        void OnKilledEnemy(int enemID);

        #endregion
        #region レリック関連
        /// <summary>
        /// レリック生成通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="relicDatas">レリックリスト</param>
        void OnDropRelic(List<DropRelicData >relicDatas);

        /// <summary>
        /// レリック取得通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="relicID">レリックID</param>
        /// <param name="rekicName">レリック名</param>
        /// </summary>
        void OnGetRelic(int relicID, string rekicName);
        #endregion
        #region ゲーム内UI、仕様
        /// <summary>
        /// ギミック起動通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="gimmickData">ギミックデータ</param>
        void OnBootGimmick(GimmickData gimmickData);

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
        void OnAdanceNextStage(Guid conID, bool isAdvance, EnumManager.STAGE_TYPE stageType);

        /// <summary>
        /// タイマー通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="timerType">タイマー辞典</param>
        void OnTimer(Dictionary<EnumManager.TIME_TYPE, int> timerType);
        #endregion
        #endregion

        /// <summary>
        /// マスタークライアントの変更通知
        /// </summary>
        void OnChangeMasterClient();

        /// <summary>
        /// ステージ進行通知
        /// Author;Nishiura
        /// </summary>
        /// <param name="conID">接続ID</param>
        void OnAdvancedStage(Guid conID);

        void OnGameEnd(ResultData result);

        /// <summary>
        /// アイテム獲得通知
        /// </summary>
        /// <param name="itemID">アイテム識別ID(文字列)</param>
        void OnGetItem(string itemID);

        #region 不要になりそうなAPI

        #region ゲーム内
        #region プレイヤー関連

        /// <summary>
        /// プレイヤー体力増減通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="playerID">プレイヤー識別ID</param>
        /// <param name="playerHP">プレイヤー体力</param>
        void OnPlayerHealth(int playerID, float playerHP);

        /// <summary>
        /// 経験値通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="exp">経験値</param>
        void OnEXP(int exp);

        /// <summary>
        /// レベルアップ通知
        /// </summary>
        void OnLevelUp();

        #endregion
        #region ゲーム内UI、仕様
        /// <summary>
        /// ダメージ表記通知
        /// </summary>
        /// <param name="dmg">ダメージ</param>
        void OnDamage(int dmg);
        #endregion
        #endregion

        #endregion
    }
}
