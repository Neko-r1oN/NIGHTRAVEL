<?php

namespace Database\Seeders;

use App\Models\Relic;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class RelicsTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        Relic::create([
            'name' => 'アタックチップ',
            'const_effect' => 0,
            'rate_effect' => 1.05,
            'calculation_method' => 1,
            'max' => 0,
            'status_type' => 'Power',
            'explanation' => '攻撃力上昇+5%',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'ディフェンスチップ',
            'const_effect' => 0,
            'rate_effect' => 1.03,
            'calculation_method' => 1,
            'max' => 0,
            'status_type' => 'Defense',
            'explanation' => '防御力上昇+3%',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'ムーブスピードスチップ',
            'const_effect' => 2,
            'rate_effect' => 1,
            'calculation_method' => 1,
            'max' => 0,
            'status_type' => 'MoveSpeed',
            'explanation' => '移動速度+2',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'アタックスピードスチップ',
            'const_effect' => 0.05,
            'rate_effect' => 1,
            'calculation_method' => 1,
            'max' => 0,
            'status_type' => 'AttackSpeedFactor',
            'explanation' => '攻撃速度+0.05',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => '冷却ファン',
            'const_effect' => 0,
            'rate_effect' => 1.15,
            'calculation_method' => 1,
            'max' => 1.15,
            'status_type' => 'GiveDebuffRates',
            'explanation' => '攻撃に凍結効果を付与　15%で発動',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => '加熱ファン',
            'const_effect' => 0,
            'rate_effect' => 1.15,
            'calculation_method' => 1,
            'max' => 1.15,
            'status_type' => 'GiveDebuffRates',
            'explanation' => '攻撃に炎上効果を付与　15%で発動',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => '液漏れ電池',
            'const_effect' => 0,
            'rate_effect' => 1.15,
            'calculation_method' => 1,
            'max' => 1.15,
            'status_type' => 'GiveDebuffRates',
            'explanation' => '攻撃に感電効果を付与　15%で発動',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'ビットコイン',
            'const_effect' => 0,
            'rate_effect' => 1.15,
            'calculation_method' => 2,
            'max' => 0,
            'status_type' => 'PierceRate',
            'explanation' => '経験値獲得量が30%増加',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'リゲインコード',
            'const_effect' => 0,
            'rate_effect' => 1.02,
            'calculation_method' => 1,
            'max' => 0,
            'status_type' => 'DmgHealRate',
            'explanation' => '与えたダメージの10%分回復する',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'スキャターバグ',
            'const_effect' => 1,
            'rate_effect' => 1,
            'calculation_method' => 1,
            'max' => 0,
            'status_type' => 'BombCnt',
            'explanation' => '攻撃時に2つボム(PLの攻撃力30%)をばらまく。',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'ホログラムアーマー',
            'const_effect' => 0,
            'rate_effect' => 1.15,
            'calculation_method' => 2,
            'max' => 0,
            'status_type' => 'Defense',
            'explanation' => '攻撃を15%の確率で回避する',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'マウス',
            'const_effect' => 0,
            'rate_effect' => 1.10,
            'calculation_method' => 2,
            'max' => 0,
            'status_type' => 'DodgeRate',
            'explanation' => '20%の確率でスキルのクールダウンをリセット',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'デジタルミート',
            'const_effect' => 1,
            'rate_effect' => 1,
            'calculation_method' => 1,
            'max' => 0,
            'status_type' => 'HealMeatCnt',
            'explanation' => '20秒ごとに最大HP5%を回復する肉塊を3つ近くに生成する　15秒経過で消滅する',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'ファイアウォール',
            'const_effect' => 1,
            'rate_effect' => 1.20,
            'calculation_method' => 2,
            'max' => 0,
            'status_type' => 'DmgResistRate',
            'explanation' => '被ダメージを20%軽減',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'ライフスカベンジャー',
            'const_effect' => 0,
            'rate_effect' => 1.002,
            'calculation_method' => 1,
            'max' => 1.03,
            'status_type' => 'KillHpReward',
            'explanation' => '敵撃破時にHPを1%分回復する',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'ラグルーター',
            'const_effect' => 0,
            'rate_effect' => 1.10,
            'calculation_method' => 2,
            'max' => 1.0,
            'status_type' => 'DARate',
            'explanation' => '攻撃時に2回ヒットする。(2回目の攻撃はダメージ50%)',
            'rarity' => 3,
        ]);
        Relic::create([
            'name' => 'バックアップHDMI',
            'const_effect' => 1,
            'rate_effect' => 0,
            'calculation_method' => 1,
            'max' => 1.0,
            'status_type' => 'HP',
            'explanation' => '一度復活する　その後破壊される',
            'rarity' => 3,
        ]);
        Relic::create([
            'name' => '識別AI',
            'const_effect' => 0,
            'rate_effect' => 1.0,
            'calculation_method' => 1,
            'max' => 0,
            'status_type' => 'All',
            'explanation' => '状態異常が付与されている敵に対して与える',
            'rarity' => 3,
        ]);
        Relic::create([
            'name' => '段ボール人形',
            'const_effect' => 0,
            'rate_effect' => 1.05,
            'calculation_method' => 1,
            'max' => 1.50,
            'status_type' => 'Power',
            'explanation' => '相手の防御を10%無視する。',
            'rarity' => 4,
        ]);
        Relic::create([
            'name' => 'ロボコア',
            'const_effect' => 1,
            'rate_effect' => 0,
            'calculation_method' => 1,
            'max' => 6,
            'status_type' => 'ElecOrbCnt',
            'explanation' => '自身を中心に回転する電気玉を発生する(攻撃力15%)',
            'rarity' => 4,
        ]);
        Relic::create([
            'name' => '違法スクリプト',
            'const_effect' => 0,
            'rate_effect' => 1.01,
            'calculation_method' => 1,
            'max' => 0,
            'status_type' => 'Power',
            'explanation' => 'ボスを除いて3%の確率で99999ダメージを与える',
            'rarity' => 4,
        ]);
        Relic::create([
            'name' => 'Caracal.png',
            'const_effect' => 0,
            'rate_effect' => 1,
            'calculation_method' => 1,
            'max' => 0,
            'status_type' => 'All',
            'explanation' => '効果なし うざい',
            'rarity' => 5,
        ]);
    }
}
