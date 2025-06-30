////////////////////////////////////////////////////////////////
///
/// 実績のカラム設定エンティティ
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
    /// 実績のカラム設定(public)
    /// </summary>
    public class Achievement
    {
        public int id { get; set; }                         //実績のID
        public string condition { get; set; }               //実績の条件テキスト
        public string name { get; set; }                    //実績の名前
        public int condition_complete { get; set; }         //実績の達成条件値
        public string type { get; set; }                    //実績のタイプ
        public DateTime Created_at { get; set; }            //生成日時
        public DateTime Updated_at { get; set; }            //更新日時
    }
}
