////////////////////////////////////////////////////////////////
///
/// ステージのカラム設定エンティティ
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
    /// ステージのカラム設定(public)
    /// </summary>
    public class Stage
    {
        public int id { get; set; }                         //ステージのID
        public string name { get; set; }                    //ステージの名前
        public string descriptive_text { get; set; }        //ステージの説明文
        public DateTime Created_at { get; set; }            //生成日時
        public DateTime Updated_at { get; set; }            //更新日時
    }
}
