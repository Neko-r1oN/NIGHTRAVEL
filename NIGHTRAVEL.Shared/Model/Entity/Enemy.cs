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

namespace NIGHTRAVEL.Shared.Model.Entity
{   
     /// <summary>
     /// 敵のカラム設定(public)
     /// </summary>
    public class Enemy
    {
        public int id {  get; set; }                        //敵のID
        public string name { get; set; }                    //敵の名前
        public double hp { get; set; }                      //敵のHP
        public double attack { get; set; }                  //敵の攻撃力
        public double defence { get; set; }                 //敵の防御力
        public double move_speed { get; set; }              //敵の移動速度
        public int stage_id { get; set; }                   //ステージのID
        public DateTime Created_at { get; set; }            //生成日時
        public DateTime Updated_at { get; set; }            //更新日時
    }
}
