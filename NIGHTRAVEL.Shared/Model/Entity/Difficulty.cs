////////////////////////////////////////////////////////////////
///
/// 難易度のカラム設定エンティティ
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
    /// 難易度のカラム設定(public)
    /// </summary>
    public class Difficulty
    {
        public int id {  get; set; }                        //難易度のID
        public string name { get; set; }                    //難易度の名前
        public int conditions { get; set; }                 //条件値
        public DateTime Created_at { get; set; }            //生成日時
        public DateTime Updated_at { get; set; }            //更新日時
    }
}
