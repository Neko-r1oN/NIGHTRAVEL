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

namespace Shared.Interfaces.StreamingHubs
{
    public interface IRoomHubReceiver
    {
        //ここにサーバー～クライアントの定義

        #region アニメーション設定(列挙型)
        /// <summary>
        /// プレイヤーアニメーションの状態(列挙型)
        /// Author:Nishiura
        /// </summary>
        public enum CharacterState
        {
            Idle = 0,
            Walk,
            Run,
            Attack,
            SecAttack,
            Hit,
            Dead,
        }

        /// <summary>
        /// 敵アニメーションの状態(列挙型)
        /// Author:Nishiura
        /// </summary>
        public enum EnemyAnimState
        {
            Idle = 0,
            Walk,
            Run,
            Attack,
            Hit,
            Dead,
        }
        #endregion

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

        /// <summary>
        /// プレイヤー死亡通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="playerID">プレイヤーID</param>
        void OnPlayerDead(int playerID);
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
        /// <param name="enemID"></param>
        /// <param name="enemHP"></param>
        void OnEnemyHealth(int enemID, float enemHP);

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
        /// <param name="relicID">レリックID</param>
        /// <param name="pos">位置</param>
        void OnSpawnRelic(int relicID, Vector2 pos);

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
        /// <param name="difID">増加後難易度</param>
        void OnAscendDifficulty(int difID);

        /// <summary>
        /// 次ステージ進行通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="stageID">次ステージID</param>
        void OnAdanceNextStage(int stageID);

        /// <summary>
        /// ダメージ表記通知
        /// </summary>
        /// <param name="dmg">ダメージ</param>
        void OnDamage(int dmg);
        #endregion
        #endregion

        /// <summary>
        /// マスタークライアントの変更通知
        /// </summary>
        void OnChangeMasterClient();
    }
}
