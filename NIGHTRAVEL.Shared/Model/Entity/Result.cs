////////////////////////////////////////////////////////////////
///
/// リザルトのカラム設定エンティティ
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Model.Entity
{
    /// <summary>
    /// リザルトのカラム設定
    /// </summary>
    public class Result
    {
        public int id { get; set; }                       //リザルトのID
        public int user_id { get; set; }                  //ユーザーのID
        public int title_id { get; set; }                 //タイトルのID
        public int wepon_id { get; set; }                 //武器のID
        public int stage_id { get; set; }                 //ステージのID
        public int difficulty_id { get; set; }            //難易度のID
        public bool is_game_clear { get; set; }           //ゲームをクリアしたか
        public int total_score { get; set; }              //総合得点
        public int total_kill { get; set; }               //総合キル数
        public int character_level { get; set; }          //キャラのレベル
        public TimeSpan alive_time { get; set; }          //生存時間
        public int max_given_damage { get; set; }         //敵に与えた最高ダメージ
        public int given_damage { get; set; }             //敵に与えた総合ダメージ
        public int received_damage { get; set; }          //敵から受けたダメージ
        public int stage_exit_count { get; set; }         //通過したしたステージの数
        public int relic_count { get; set; }              //集めたレリックの数
        public int power_up_count { get; set; }           //ステータス強化の回数
        public double move_distance { get; set; }         //移動距離
        public int boss_kill_count { get; set; }          //倒したボスの数
        public int stage_complete { get; set; }           //クリアしたステージの数
        public TimeSpan play_time { get; set; }           //このリザルトにおいてのプレイ時間
        public int dead_count { get; set; }               //死亡数
        public DateTime Created_at { get; set; }          //生成日時
        public DateTime Updated_at { get; set; }          //更新日時

    }
}
