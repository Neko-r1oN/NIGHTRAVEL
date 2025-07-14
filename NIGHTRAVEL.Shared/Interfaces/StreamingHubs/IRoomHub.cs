////////////////////////////////////////////////////////////////
///
/// クライアントからサーバーへの通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

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
        Task MovePlayerAsync(Vector3 pos, Quaternion rot, CharacterState anim);

        /// <summary>
        /// 敵動作、情報
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemIDList">敵ID</param>
        /// <param name="pos">敵位置値</param>
        /// <param name="rot">敵回転値</param>
        /// <param name="anim">敵アニメーションID</param>
        /// <returns></returns>
        Task MoveEnemyAsync(List<int> enemIDList, Vector3 pos, Quaternion rot, EnemyAnimState anim);

        ////敵の出現処理
        //Task SpawnAsync(string enemyName, Vector3 pos);

        ////敵のID同期
        //Task EnemyIdAsync(int enemyid);

        ////敵の位置回転
        //Task EnemyMoveAsync(string enemyName,Vector3 pos,Quaternion rot);

        ////敵の撃破処理
        //Task EnemyExcusionAsync(string enemyName);

        ////マスタークライアントが退室したときの処理
        //Task MasterLostAsync();


        ////オブジェクトの生成同期
        //Task ObjectSpawnAsync(Guid connectionId,string objectName,Vector3 pos,Quaternion rot, Vector3 fow);

        ////オブジェクトの位置回転同期
        //Task ObjectMoveAsync(string objectName,Vector3 pos,Quaternion rot);
    }
}
