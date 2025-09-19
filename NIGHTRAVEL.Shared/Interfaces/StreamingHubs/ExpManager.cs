//=============================
// 経験値管理スクリプト
// Author:Nishiura
//=============================

using MessagePack;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class ExpManager
    {
        /// <summary>
        /// 現在のレベル
        /// Author:Nishiura
        /// </summary>
        [Key(0)]
        public int Level { get; set; } = 1;

        /// <summary>
        /// 必要経験値
        /// Author:Nishiura
        /// </summary>
        [Key(1)]
        public int RequiredExp { get; set; } = 10;     // 初回必要な経験値量

        /// <summary>
        /// 所持経験値
        /// Author:Nishiura
        /// </summary>
        [Key(2)]
        public int nowExp { get; set; } = 0;
    }
}
