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
            'effect' => 0.15,
            'explanation' => '攻撃に凍結効果を付与　15%で発動',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => '加熱ファン',
            'effect' => 0.15,
            'explanation' => '攻撃に炎上効果を付与　15%で発動',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => '液漏れ電池',
            'effect' => 0.15,
            'explanation' => '攻撃に感電効果を付与　15%で発動',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'ビットコイン',
            'effect' => 1.30,
            'explanation' => '経験値獲得量が30%増加',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'リゲインコード',
            'effect' => 1.02,
            'explanation' => '与えたダメージの10%分回復する',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'スキャターバグ',
            'effect' => 1.05,
            'explanation' => '攻撃時に2つボム(PLの攻撃力30%)をばらまく。',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'ホログラムアーマー',
            'effect' => 1.15,
            'explanation' => '攻撃を15%の確率で回避する',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'マウス',
            'effect' => 1.10,
            'explanation' => '20%の確率でスキルのクールダウンをリセット',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'デジタルミート',
            'effect' => 3,
            'explanation' => '20秒ごとに最大HP5%を回復する肉塊を3つ近くに生成する　15秒経過で消滅する',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'ファイアウォール',
            'effect' => 1.20,
            'explanation' => '被ダメージを20%軽減',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'B.A.Nハンマー',
            'effect' => 1.025,
            'explanation' => '経験値獲得量が30%増加',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'ライフスカベンジャー',
            'effect' => 1.005,
            'explanation' => '敵撃破時にHPを1%分回復する',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'ラグルーター',
            'effect' => 1.10,
            'explanation' => '攻撃時に2回ヒットする。(2回目の攻撃はダメージ50%)',
            'rarity' => 3,
        ]);
        Relic::create([
            'name' => 'バックアップHDMI',
            'effect' => 1,
            'explanation' => '一度復活する　その後破壊される',
            'rarity' => 3,
        ]);
        Relic::create([
            'name' => '識別AI',
            'effect' => 1,
            'explanation' => '状態異常が付与されている敵に対して与える',
            'rarity' => 3,
        ]);
        Relic::create([
            'name' => '帯電コア',
            'effect' => 8,
            'explanation' => '攻撃ヒット時、基礎攻撃力の800%のダメージを与える広範囲の電磁波を放つ　クールダウン10秒',
            'rarity' => 4,
        ]);
        Relic::create([
            'name' => 'Caracal.png',
            'effect' => 1.10,
            'explanation' => '効果なし うざい',
            'rarity' => 5,
        ]);
    }
}
