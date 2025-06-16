////////////////////////////////////////////////////////////////
///
/// ユーザーのカラム設定エンティティ
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using System;

namespace NIGHTRAVEL.Server.Model.Entity
{
    /// <summary>
    /// ユーザーのカラム設定(Public)
    /// </summary>
    public class User
    {
        public int id { get; set; }                         //ユーザーのid
        public DateTime Created_at { get; set; }            //生成日時
        public DateTime Updated_at { get; set; }            //更新日時

    }
}
