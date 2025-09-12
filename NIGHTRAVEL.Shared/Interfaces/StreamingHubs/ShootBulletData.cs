using System;
using System.Collections.Generic;
using MessagePack;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class ShootBulletData
    {
        [Key(0)]
        public EnumManager.PROJECTILE_TYPE Type { get; set; }

        [Key(1)]
        public List<EnumManager.DEBUFF_TYPE> Debuffs { get; set; } = new List<EnumManager.DEBUFF_TYPE>();

        [Key(2)]
        public int Power { get; set; }

        [Key(3)]
        public Vector2 SpawnPos { get; set; }

        [Key(4)]
        public Vector2 ShootVec { get; set; }

        [Key(5)]
        public Quaternion Rotation { get; set; }
    }
}
