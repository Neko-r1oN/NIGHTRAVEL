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
        /// プレイヤー動作、情報
        /// Author:Nishiura
        /// </summary>
        /// <param name="pos">PL位置値</param>
        /// <param name="rot">Pl回転値</param>
        /// <param name="anim">PLアニメーションID</param>
        /// <returns></returns>
        Task MovePlayerAsync(Vector2 pos, Quaternion rot, CharacterState anim);

        /// <summary>
        /// 敵動作、情報
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemIDList">敵ID</param>
        /// <param name="pos">敵位置値</param>
        /// <param name="rot">敵回転値</param>
        /// <param name="anim">敵アニメーションID</param>
        /// <returns></returns>
        Task MoveEnemyAsync(List<int> enemIDList, Vector2 pos, Quaternion rot, EnemyAnimState anim);

        /// <summary>
        /// レリック位置
        /// Author:Nishiura
        /// </summary>
        /// <param name="pos">位置</param>
        /// <returns></returns>
        Task SpawnRelicAsync(Vector2 pos);

        /// <summary>
        /// レリック取得
        /// </summary>
		/// <param name="relicID">レリックID</param>
        /// <param name="relicName">レリック名</param>
        /// <returns></returns>
        Task GetRelicAsync(int relicID, string relicName);

        /// <summary>
        /// 敵生成
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <param name="pos">位置</param>
        /// <returns></returns>
        Task SpawnEnemyAsync(int enemID, Vector2 pos);

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
        /// 敵体力増減
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <param name="enemHP">敵体力</param>
        /// <returns></returns>
        Task EnemyHealthAsync(int enemID, float enemHP);

        ////敵のID同期
        //Task EnemyIdAsync(int enemyid);

        ////敵の撃破処理
        //Task EnemyExcusionAsync(string enemyName);

        ////マスタークライアントが退室したときの処理
        //Task MasterLostAsync();
    }
}
