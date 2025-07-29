//=============================
// 敵へのダメージのデータスクリプト
// Author:木田晃輔 Data:07/29
//=============================
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace NIGHTRAVEL.Shared.Interfaces.StreamingHubs
{
    internal class EnemyDamegeData
    {
        // オブジェクト名
        public string EnemyObjectName { get; set; }

        // プレイヤーのtrancefome
        public Vector2 PlayerPosition { get; set; }

        // 状態異常リスト
        public List<int> DebuffList {  get; set; }

        //　残りHP
        public int RemainingHp { get; set; }

        // 経験値
        public int Exp {  get; set; }
    }
}
