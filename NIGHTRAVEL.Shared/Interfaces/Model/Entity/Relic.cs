////////////////////////////////////////////////////////////////
///
/// レリックのカラム設定エンティティ
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
    /// レリックのカラム設定
    /// </summary>
    public class Relic
    {
        public int id { get; set; }                     //レリックのID
        public string name { get; set; }                //レリックの名前
        public int const_effect { get; set; }        //レリックの効果量
        public float rate_effect { get; set; }         //レリックの効果量
        public int max {  get; set; }                   //上昇ステータス最大値
        public int status_type {  get; set; }           //上昇ステータスの種類
        public string explanation { get; set; }         //レリックの説明文
        public int rarity { get; set; }                 //レリックのレア度
        public DateTime Created_at { get; set; }            //生成日時
        public DateTime Updated_at { get; set; }            //更新日時
    }
}
