//=============================
// 敵へのダメージのデータスクリプト
// Author:木田晃輔 Data:07/29
//=============================
using MessagePack;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class EnemyDamegeData
    {
        //// ダメージを付与する敵のID
        //public string EnemyObjectName { get; set; }

        //// 攻撃したプレイヤーのID
        //public Vector2 PlayerPosition { get; set; }

        //// 付与する状態異常リスト
        //public List<int> DebuffList {  get; set; }

        //// 残りHP
        //public int RemainingHp { get; set; }

        //// 敵の経験値
        //public int Exp {  get; set; }
    }
}
