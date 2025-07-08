////////////////////////////////////////////////////////////////
///
/// ユーザーのカラム設定エンティティ
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using MessagePack;
using System;

namespace NIGHTRAVEL.Shared.Interfaces.Model.Entity
{
    [MessagePackObject]
    /// <summary>
    /// ユーザーのカラム設定(Public)
    /// </summary>
    public class User
    {
        [Key(0)]
        public int Id { get; set; }                         //ユーザーのid
        [Key(1)]
        public string Name { get; set; }                    //名前
        [Key(2)]
        public DateTime Created_at { get; set; }            //生成日時
        [Key(3)]
        public DateTime Updated_at { get; set; }            //更新日時

    }
}
