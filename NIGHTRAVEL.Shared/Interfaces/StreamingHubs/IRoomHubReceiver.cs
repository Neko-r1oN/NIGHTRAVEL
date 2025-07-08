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

namespace Shared.Interfaces.StreamingHubs
{
    public interface IRoomHubReceiver
    {
        //ここにサーバー～クライアントの定義

        //ユーザーの入室通知
        void Onjoin(Dictionary<Guid, JoinedUser> joindUserList);

        //ユーザーの退室通知
        void OnLeave(JoinedUser user);

        //////プレイヤーアニメーションの状態(列挙型)
        ////public enum CaracterState
        ////{
        ////    Idol=0,
        ////    Wark,
        ////    Idol=1,
        ////    Run
        ////}

        ////プレイヤーの位置・回転・アニメーション同期
        //void OnMove(JoinedUser user,Vector3 pos,Quaternion rot,int anim);

        ////敵のスポーン
        //void OnSpawn(string enemyName, Vector3 pos);

        ////敵のID同期
        //void OnIdEnemy(int enemyId);

        ////敵の移動同期
        //void OnMoveEnemy( string enemyName,Vector3 pos, Quaternion rot);

        ////敵の撃破処理
        //void OnExcusionEnemy(string enemyName);

        ////マスタークライアント譲渡処理
        //void OnMasterClient(JoinedUser joinedUser);

        ////オブジェクトの生成同期
        //void OnObjectSpawn(Guid connectionId,string objectName, Vector3 pos,Quaternion rot,Vector3 fow);

        ////オブジェクトの移動回転同期
        //void OnObjectMove(string objectName, Vector3 pos, Quaternion rot);
    }
}
