////////////////////////////////////////////////////////////////
///
/// ステータス強化のカラム設定エンティティ
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
    /// ステータス強化のカラム設定(Public)
    /// </summary>
    public class Status_Enhancement
    {
        public int id { get; set; }                         //ステータス強化のid
        public string name { get; set; }                    //ステータス強化の名前
        public int rarity { get; set; }                    //ステータス強化のレア度
        public string explanation { get; set; }            //ステータス強化の説明文
        public int type { get; set; }                      //ステータス強化タイプ
        public double const_effect1 { get; set; }          //実数値１つ目の効果量
        public double rate_effect1 { get; set; }          //加算する割合1つ目の効果量
        public double const_effect2 { get; set; }          //実数値2つ目の効果量
        public double rate_effect2 { get; set; }          //加算する割合2つ目の効果量
        public string enhancement_type { get; set; }       //ステータス強化の識別名
        public bool duplication { get; set; }              //重複の判定
        public DateTime Created_at { get; set; }            //生成日時
        public DateTime Updated_at { get; set; }            //更新日時
    }
}
