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
        /// <summary>
        /// ダメージを付与する敵のID
        /// </summary>
        public int HitEnemyId { get; set; }

        [Key(1)]
        /// <summary>
        /// 攻撃したプレイヤーのID
        /// </summary>
        public Guid AttackerId { get; set; }

        [Key(2)]
        /// <summary>
        /// 付与する状態異常のリスト
        /// </summary>
        public List<DEBUFF_TYPE> DebuffList { get; set; }

        [Key(3)]
        /// <summary>
        /// 敵の残りHP
        /// </summary>
        public int RemainingHp { get; set; }

        [Key(4)]
        /// <summary>
        /// 敵へのダメージ量
        /// </summary>
        public int Damage { get; set; }
    }
}
