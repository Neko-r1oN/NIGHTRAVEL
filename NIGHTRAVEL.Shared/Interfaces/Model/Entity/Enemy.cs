////////////////////////////////////////////////////////////////
///
/// 敵のカラム設定エンティティ
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Interfaces.Model.Entity
{
    /// <summary>
    /// 敵のカラム設定(public)
    /// </summary>
    public class Enemy
    {
        public int id { get; set; }                        //敵のID
        public string name { get; set; }                    //敵の名前
        public bool isBoss { get; set; }                    //ボスかどうか
        public int hp { get; set; }                      //敵のHP
        public double defence { get; set; }                 //敵の防御力
        public double power { get; set; }                  //敵の攻撃力
        public double jump_power { get; set; }              //敵の跳躍力
        public double move_speed { get; set; }              //敵の移動速度
        public double attack_speed_factor { get; set; }           //敵の攻撃速度
        public string stage_id { get; set; }                //ステージのID(識別用)
        public int exp { get; set; }                        //経験値
        public DateTime Created_at { get; set; }            //生成日時
        public DateTime Updated_at { get; set; }            //更新日時
    }
}
