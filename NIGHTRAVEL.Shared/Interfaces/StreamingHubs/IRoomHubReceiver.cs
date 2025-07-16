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
        /// プレイヤー動作通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="user">対象者</param>
        /// <param name="pos">位置</param>
        /// <param name="rot">回転</param>
        /// <param name="animID">アニメーションID</param>
        void OnMovePlayer(JoinedUser user, Vector2 pos, Quaternion rot, CharacterState animID);

		/// <summary>
		/// 敵の動作通知
		/// Author:Nishiura
		/// </summary>
		/// <param name="enemID">対象敵ID</param>
		/// <param name="pos">位置</param>
		/// <param name="rot">回転</param>
		/// <param name="animID">アニメーションID</param>
		void OnMoveEnemy(int enemID, Vector2 pos, Quaternion rot, EnemyAnimState animID);

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

        /// <summary>
        /// 敵生成通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemData">敵情報</param>
        /// <param name="pos">位置</param>
        void OnSpawnEnemy(EnemyData enemData, Vector2 pos);

        /// <summary>
        /// ギミック起動通知
        /// Author:Nishiura
        /// </summary>
        /// <param name="gimmickData">ギミックデータ</param>
        void OnBootGimmick(GimmickData gimmickData);

        ////敵のID同期
        //void OnIdEnemy(int enemyId);

        ////敵の移動同期
        //void OnMoveEnemy( string enemyName,Vector2 pos, Quaternion rot);

        ////敵の撃破処理
        //void OnExcusionEnemy(string enemyName);

        ////マスタークライアント譲渡処理
        //void OnMasterClient(JoinedUser joinedUser);

        ////オブジェクトの生成同期
        //void OnObjectSpawn(Guid connectionId,string objectName, Vector2 pos,Quaternion rot,Vector2 fow);

        ////オブジェクトの移動回転同期
        //void OnObjectMove(string objectName, Vector2 pos, Quaternion rot);
    }
}
