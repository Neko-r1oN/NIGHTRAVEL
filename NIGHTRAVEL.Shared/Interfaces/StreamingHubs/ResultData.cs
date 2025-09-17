//=============================
// リザルトのデータスクリプト
// Author:木田晃輔 Data:07/29
//=============================
using MessagePack;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class ResultData
    {
        [Key(0)]
        /// <summary>
        /// プレイヤーのクラス
        /// </summary>
        public EnumManager.Player_Type PlayerClass { get; set; }

        [Key(1)]
        /// <summary>
        /// 取得レリックリスト
        /// </summary>
        public List<EnumManager.RELIC_TYPE> GottenRelicList { get; set; }

        [Key(2)]
        /// <summary>
        /// 攻略ステージ数
        /// </summary>
        public int TotalClearStageCount { get; set; }

        [Key(3)]
        /// <summary>
        /// 到達レベル
        /// </summary>
        public int DifficultyLevel { get; set; }

        [Key(4)]
        /// <summary>
        /// 生存時間
        /// </summary>
        public TimeSpan AliveTime { get; set; }

        [Key(5)]
        /// <summary>
        /// エネミー討伐数
        /// </summary>
        public int EnemyKillCount { get; set; }

        [Key(6)]
        /// <summary>
        /// 総付与ダメージ
        /// </summary>
        public int TotalGaveDamage { get; set; }

        [Key(7)]
        /// <summary>
        /// 総獲得アイテム数
        /// </summary>
        public int TotalGottenItem { get; set; }

        [Key(8)]
        /// <summary>
        /// 合計端末起動数
        /// </summary>
        public int TotalActivedTerminal { get; set; }

        [Key(9)]
        /// <summary>
        /// 合計スコア
        /// </summary>
        public int TotalScore { get; set; }
    }
}
