//=============================
// プレイヤーデータスクリプト
// Author:木田晃輔
//=============================
using MessagePack;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class EnumManager
    {
        #region システム関連

        /// レア度の種類
        /// Author:木田晃輔
        /// </summary>
        public enum RARITY_TYPE
        {
            Common = 1,
            Uncommon,
            Rare,
            Unique,
            Legend,
            Boss
        }

        /// <summary>
        /// タイマ-の種類
        /// Author:Nishiura
        /// </summary>
        public enum TIME_TYPE
        {
            GameTimer,          // ゲーム全体のタイマー
            TerminalTimer       // 端末のレース用タイマー
        }

        /// <summary>
        /// 難易度の種類
        /// Author:Nishiura
        /// </summary>
        public enum DIFFICULTY_TYPE
        {
            Baby,
            Easy,
            Normal,
            Hard,
            VeryHard,
            Hell
        }

        /// <summary>
        /// ステージ種類
        /// Author:Nishiura
        /// </summary>
        public enum STAGE_TYPE
        {
            Rust,
            Industry,
            Town,
            FinalBoss,
        }

        /// <summary>
        /// アイテムの種類
        /// </summary>
        public enum ITEM_TYPE
        {
            Relic,
            DataCube,
            DataBox
        }

        /// <summary>
        /// 発射物の種類
        /// </summary>
        public enum PROJECTILE_TYPE
        {
            BoxBullet,
            MissileBullet,
            NodeBullet_N,
            NodeBullet_O,
            NodeBullet_D,
            NodeBullet_E,
            BoxBullet_Big,
        }

        /// <summary>
        /// オブジェクトの種類
        /// </summary>
        public enum OBJECT_TYPE
        {
            Box,
        }

        /// <summary>
        /// 計算方法の種類
        /// </summary>
        public enum CalculationType
        {
            Additive = 0,   // 加算 (+10, +20など)
            Multiplicative // 乗算 (×1.1, ×1.5など)
        }

        /// <summary>
        /// 端末の種類
        /// </summary>
        public enum TERMINAL_TYPE
        {
            None = 0,
            Enemy,
            Speed,
            Deal,
            Elite,
            Jumble,
            Boss
        }

        /// <summary>
        /// 端末の状態
        /// </summary>
        public enum TERMINAL_STATE
        {
            None = 0,
            Inactive,   // 非アクティブ状態
            Active,     // アクティブ状態
            Success,    // 成功状態
            Failure     // 失敗状態
        }

        #endregion

        #region キャラクター関連

        /// <summary>
        /// 状態異常の種類
        /// </summary>
        public enum DEBUFF_TYPE
        {
            None = 0,
            Burn,       // 炎上状態
            Freeze,     // 霜焼け状態
            Shock       // 感電状態
        }

        /// <summary>
        /// ステータス強化の選択肢の種類
        /// DB上のレコードのIDと完全一致させる
        /// </summary>
        public enum STAT_UPGRADE_OPTION 
        {
            Common_HP = 1,
            Common_Deffence,
            Common_Attack,
            Common_JumpingPower,
            Common_MovementSpeed,
            Common_AttackSpeed,
            Common_AutomaticRecovery,
            Uncommon_HP,
            Uncommon_Deffence,
            Uncommon_Attack,
            Uncommon_JumpingPower,
            Uncommon_MovementSpeed,
            Uncommon_AttackSpeed,
            Uncommon_AutomaticRecovery,
            Rare_HP,
            Rare_Deffence,
            Rare_Attack,
            Rare_JumpingPower,
            Rare_MovementSpeed,
            Rare_AttackSpeed,
            Rare_AutomaticRecovery,
            Unique_HP,
            Unique_Deffence,
            Unique_Attack,
            Unique_JumpingPower,
            Unique_MovementSpeed,
            Unique_AttackSpeed,
            Unique_AutomaticRecovery,
            Legend_HP,
            Legend_Deffence,
            Legend_Attack,
            Legend_JumpingPower,
            Legend_MovementSpeed,
            Legend_AttackSpeed,
            Legend_AutomaticRecovery
        }

        /// <summary>
        /// ステータスの種類
        /// </summary>
        public enum STATUS_TYPE
        {
            All,
            HP,
            Defense,
            Power,
            JumpPower,
            MoveSpeed,
            AttackSpeedFactor,
            HealRate,
            
            // レリックステータス (PlayerRelicStatusDataのプロパティ参照)
            BurningRate,
            CoolingRate,
            ShokingRate,
            AddExpRate,
            RegainCodeRate,
            ScatterBugCnt,
            HolographicArmorRate,
            MouseRate,
            DigitalMeatCnt,
            FirewallRate,
            LifeScavengerRate,
            RugrouterRate,
            BuckupHDMICnt,
            IdentificationAIRate,
            DanborDollRate,
            ChargedCoreCnt,
            IllegalScriptRate
        }

        /// <summary>
        /// レリックの種類
        /// </summary>
        public enum RELIC_TYPE
        {
            AttackTip = 1,
            DeffenceTip,
            MoveSpeedTip,
            AttackSpeedTip,
            CoolingFan,
            HeatingFan,
            LeakingBattery,
            BitCoin,
            RegainCode,
            ScatterBug,
            HolographicArmor,
            Mouse,
            DigitalMeat,
            Firewall,
            LifeScavenger,
            Rugrouter,
            BuckupHDMI,
            IdentificationAI,
            DanborDoll,
            ChargedCore,
            IllegalScript,
            CaracalPng
        }

        #region 操作キャラ関連

        /// <summary>
        /// 操作キャラの種類
        /// </summary>
        public enum Player_Type
        {
            Sword,
            Gunner
        }

        #endregion
        #region 敵関連

        /// <summary>
        /// 敵の種類
        /// </summary>
        public enum ENEMY_TYPE
        {
            // ステージ１
            Hakonov_TypeA = 1,
            Hakonov_TypeB,
            Hakonov_TypeC,
            Signabot,
            Delibot,
            Vendbot,
            Boxgeist,

            // ステージ２
            Drone,
            Guardbot,
            CyberDog,
            MissileMachine,
            CyberDog_ByWorm,
            Drone_ByWorm,
            FullMetalWorm,
            MetalBody,

            // ステージ３
            Carcass,
            Node_Core,
            Slade,
            Blaze,
            Valcus,
            ValksCodecrysta,
        }

        /// <summary>
        /// 敵の生成タイプ
        /// </summary>
        public enum SPAWN_ENEMY_TYPE
        {
            ByManager,      // Managerによる生成
            ByTerminal,     // 端末による生成
            ByWorm,         // ワームによる生成
        }

        /// <summary>
        /// エリートの種類
        /// </summary>
        public enum ENEMY_ELITE_TYPE
        {
            None,
            Blaze,      // ブレイズエリート
            Frost,      // フロストエリート
            Thunder     // サンダーエリート
        }

        #endregion

        #endregion
    }
}
