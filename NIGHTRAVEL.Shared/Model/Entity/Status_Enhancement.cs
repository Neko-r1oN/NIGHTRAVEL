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

namespace NIGHTRAVEL.Shared.Model.Entity
{
    /// <summary>
    /// ステータス強化のカラム設定(Public)
    /// </summary>
    public class Status_Enhancement
    {
        public int id { get; set; }                         //ステータス強化のid
        public string name { get; set; }                    //ステータス強化の名前
        public int rarity {  get; set; }                    //ステータス強化のレア度
        public string explanation {  get; set; }            //ステータス強化の説明文
        public int type {  get; set; }                      //ステータス強化タイプ
        public double effect {  get; set; }                  //効果量
        public string enhancement_type { get; set; }        //ステータス強化の識別名
        public bool duplication {  get; set; }              //重複の判定
        public DateTime Created_at { get; set; }            //生成日時
        public DateTime Updated_at { get; set; }            //更新日時
    }
}
