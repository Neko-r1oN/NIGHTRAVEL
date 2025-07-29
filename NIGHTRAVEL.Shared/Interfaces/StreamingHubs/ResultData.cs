//=============================
// リザルトのデータスクリプト
// Author:木田晃輔 Data:07/29
//=============================
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Interfaces.StreamingHubs
{
    internal class ResultData
    {
        //レベル
        public int Level {  get; set; }

        //生存時間
        public int AliveTime {  get; set; }

        //死亡回数
        public int DeathCount {  get; set; }

        //キル数
        public int EnemyKillCount {  get; set; }

        //キル数(ボス)
        public int BossKillCount { get; set; }

        //付与ダメージ
        public int TotalDamage { get; set; }

        //ステータス強化選択回数
        public int StatusUpCount { get; set; }
    }
}
