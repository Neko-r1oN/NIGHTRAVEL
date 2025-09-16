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
        public int id { get; set; }                         //ステージのID
        [Key(1)]
        public string roomName { get; set; }                    //ステージの名前
        [Key(2)]
        public string userName { get; set; }        //ステージの説明文
        [Key(3)]
        public DateTime Created_at { get; set; }            //生成日時
        [Key(4)]
        public DateTime Updated_at { get; set; }            //更新日時
    }
}
