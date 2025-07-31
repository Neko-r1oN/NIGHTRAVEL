//=============================
// 敵へのダメージのデータスクリプト
// Author:木田晃輔 Data:07/29
//=============================
using MessagePack;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static Shared.Interfaces.StreamingHubs.EnumManager;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class EnemyDamegeData
    {
        [Key(0)]
        // ダメージを付与する敵のID
        public int HitEnemyId { get; set; }

        [Key(1)]
        // 攻撃したプレイヤーのID
        public Guid AttackerId { get; set; }

        [Key(2)]
        // 付与する状態異常リスト
        public List<DEBUFF_TYPE> DebuffList { get; set; }

        [Key(3)]
        // 残りHP
        public int RemainingHp { get; set; }

        [Key(4)]
        // 敵の経験値
        public int Exp { get; set; }
    }
}
