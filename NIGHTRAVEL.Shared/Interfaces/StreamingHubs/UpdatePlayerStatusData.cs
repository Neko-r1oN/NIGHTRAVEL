//===============================
// プレイヤーの更新後のステータをまとめたデータクラス
// Author:Rui Enomoto
//===============================
using MessagePack;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Text;
using static Shared.Interfaces.StreamingHubs.EnumManager;

namespace NIGHTRAVEL.Shared.Interfaces.StreamingHubs
{
    public class UpdatePlayerStatusData
    {
        /// <summary>
        /// 最大ステータス
        /// </summary>
        CharacterStatusData characterStatusData { get; set; }

        /// <summary>
        /// レリック限定のステータス
        /// </summary>
        PlayerRelicStatusData playerRelicStatusData { get; set; }
    }
}
