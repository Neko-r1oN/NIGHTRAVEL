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
    public class FullMetalBodyData : EnemyData
    {
        [Key(12)]
        public List<Quaternion> GunRotations { get; set; }
    }
}
