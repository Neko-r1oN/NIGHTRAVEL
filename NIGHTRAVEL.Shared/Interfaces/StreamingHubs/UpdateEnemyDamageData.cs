//=============================
// 敵へのダメージの更新データスクリプト
// Author:木田晃輔 Data:07/29
//=============================
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NIGHTRAVEL.Shared.Interfaces.StreamingHubs
{
    internal class UpdateEnemyDamageData
    {
        //オブジェクト名
        public string EnemyObjectName { get; set; }

        //PlayerのTransform
        public Vector2 PlayerPosition { get; set; }

        //付与する状態異常のリスト
        public List<int> DebuffList { get; set; }

        //計算したダメージ量
        public int DamageCount {  get; set; }
    }
}
