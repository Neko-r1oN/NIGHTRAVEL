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
        /// プレイヤー動作、情報
        /// Author:Nishiura
        /// </summary>
        /// <param name="pos">PL位置値</param>
        /// <param name="rot">Pl回転値</param>
        /// <param name="anim">PLアニメーションID</param>
        /// <returns></returns>
        Task MovePlayerAsync(PlayerData playerData);

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

        /// <summary>
        /// プレイヤー死亡
        /// Author:Nishiura
        /// </summary>
        /// <param name="playerID">プレイヤーID</param>
        /// <returns></returns>
        Task PlayerDeadAsync(int playerID);
        #endregion
        #region 敵関連
        /// <summary>
        /// 敵動作、情報
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemyData">敵の情報</param>
        /// <returns></returns>
        Task UpdateEnemyAsync(EnemyData enemyData);

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
        /// <param name="enemHP">敵体力</param>
        /// <returns></returns>
        Task EnemyHealthAsync(int enemID, float enemHP);

        /// <summary>
        /// 敵死亡
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <returns></returns>
        Task KilledEnemyAsync(int enemID);
        #endregion
        #region レリック関連
        /// <summary>
        /// レリック位置
        /// Author:Nishiura
        /// </summary>
        /// <param name="pos">位置</param>
        /// <returns></returns>
        Task SpawnRelicAsync(Vector2 pos);

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
        Task AscendDifficultyAsync(int difID);

        /// <summary>
        /// 次ステージ進行
        /// Author:Nishiura
        /// </summary>
        /// <param name="stageID">ステージID</param>
        /// <param name="isBossStage">ボスステージ判定</param>
        /// <returns></returns>
        Task AdvanceNextStageAsync(int stageID, bool isBossStage);

        /// <summary>
        /// ダメージ表記
        ///  Author:Nishiura
        /// </summary>
        /// <param name="dmg">ダメージ</param>
        /// <returns></returns>
        Task DamageAsync(int dmg);
        #endregion
        #endregion

        /// <summary>
        /// マスタークライアントが退室したときの処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">接続ID</param>
        /// <returns></returns>
        Task MasterLostAsync(Guid conID);
    }
}
