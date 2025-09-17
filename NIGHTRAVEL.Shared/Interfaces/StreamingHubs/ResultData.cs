//=============================
// リザルトのデータスクリプト
// Author:木田晃輔 Data:07/29
//=============================
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Interfaces.StreamingHubs
{
    public class ResultData
    {
        //レベル
        public int Level {  get; set; }

        //生存時間
        public TimeSpan AliveTime {  get; set; }

        //難易度
        public int Difficulty { get; set; }

        //プレイヤーのクラス
        public EnumManager.Player_Type PlayerClass {  get; set; }

        //キル数
        public int EnemyKillCount {  get; set; }

        //取得レリックリスト
        public List<Relic> GottenRelicList { get; set; }

        //合計付与ダメージ
        public int TotalGaveDamage { get; set; }

        //合計端末機同数
        public int TotalActivedTerminal { get; set; }

        //合計獲得アイテム
        public int TotalGottenItem { get; set; }

        // 最終到達レベル
        public int MaxLevel { get; set; }

        // 合計クリアステージ数
        public int TotalClearStageCount { get; set; }

        // 合計スコア
        public int TotalScore { get; set; }
    }
}
