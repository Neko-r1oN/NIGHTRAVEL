////////////////////////////////////////////////////////////////
///
/// アカウントのカラム設定エンティティ
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
    /// アカウントのカラム設定(public)
    /// </summary>
    public class Account
    {
        public int id { get; set; }                        //アカウントのID
        public string account_name { get; set; }           //アカウントの名前
        public string password { get; set; }               //アカウントのパスワード
        public DateTime Created_at { get; set; }            //生成日時
        public DateTime Updated_at { get; set; }            //更新日時
    }
}
