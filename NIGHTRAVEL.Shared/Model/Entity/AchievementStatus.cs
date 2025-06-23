////////////////////////////////////////////////////////////////
///
/// ユーザーの実績進捗のカラム設定エンティティ
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
    /// ユーザーの実績進捗カラム設定
    /// </summary>
    public class AchievementStatus
    {
        public int id { get; set; }                       //ユーザーの実績進捗のID
        public int user_id { get; set; }                  //ユーザーのID
        public int achievement_id {  get; set; }          //実績のID
        public int progress { get; set; }                 //進捗度
    }
}
