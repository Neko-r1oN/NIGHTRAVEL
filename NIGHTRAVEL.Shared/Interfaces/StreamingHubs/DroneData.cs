//=============================
// ドローン用のデータスクリプト
// Author:Enomoto Data:07/30
//=============================
using MessagePack;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class DroneData : EnemyData
    {
        [Key(12)]
        public Quaternion GunRotation { get; set; }
    }
}
