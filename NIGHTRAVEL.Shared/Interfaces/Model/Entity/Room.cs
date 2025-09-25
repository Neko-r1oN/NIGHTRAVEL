using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Interfaces.Model.Entity
{
    [MessagePackObject]
    public class Room
    {
        [Key(0)]
        public int id { get; set; }                         //ルームのID
        [Key(1)]
        public string roomName { get; set; }                    //ルームの名前
        [Key(2)]
        public string userName { get; set; }        //ルームの説明文
        [Key(3)]
        public string password { get; set; }        //ルームのパスワード
        [Key(4)]
        public bool is_started { get; set; }        //ルームがゲーム開始しているか
        [Key(5)]
        public DateTime Created_at { get; set; }            //生成日時
        [Key(6)]
        public DateTime Updated_at { get; set; }            //更新日時
    }
}
