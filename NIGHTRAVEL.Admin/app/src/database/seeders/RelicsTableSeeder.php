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
            'rate_effect' => 0.05,
            'max' => 0,
            'status_type' => 3,
            'explanation' => '攻撃力が少し上昇',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'ディフェンスチップ',
            'const_effect' => 0,
            'rate_effect' => 0.03,
            'max' => 0,
            'status_type' => 2,
            'explanation' => '防御力が少し上昇',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'ムーブスピードスチップ',
            'const_effect' => 2,
            'rate_effect' => 0,
            'max' => 0,
            'status_type' => 5,
            'explanation' => '移動速度が少し上昇',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'アタックスピードスチップ',
            'const_effect' => 0.05,
            'rate_effect' => 0,
            'max' => 0,
            'status_type' => 6,
            'explanation' => '攻撃速度が少し上昇',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => '冷却ファン',
            'const_effect' => 0,
            'rate_effect' => 15,
            'max' => 100,
            'status_type' => 9,
            'explanation' => '攻撃時に凍結効果を確率で付与',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => '加熱ファン',
            'const_effect' => 0,
            'rate_effect' => 15,
            'max' => 100,
            'status_type' => 8,
            'explanation' => '攻撃時に炎上効果を確率で付与',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => '液漏れ電池',
            'const_effect' => 0,
            'rate_effect' => 15,
            'max' => 100,
            'status_type' => 10,
            'explanation' => '攻撃時に感電効果を確率で付与',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'ビットコイン',
            'const_effect' => 0,
            'rate_effect' => 0.15,
            'max' => 0,
            'status_type' => 11,
            'explanation' => '経験値獲得量が増加',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'リゲインコード',
            'const_effect' => 0,
            'rate_effect' => 0.02,
            'max' => 0.30,
            'status_type' => 12,
            'explanation' => '与えたダメージを少し回復する',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'スキャターバグ',
            'const_effect' => 1,
            'rate_effect' => 0,
            'max' => 0,
            'status_type' => 13,
            'explanation' => '攻撃時にボムをばらまく',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'ホログラムアーマー',
            'const_effect' => 10,
            'rate_effect' => 0,
            'max' => 100,
            'status_type' => 14,
            'explanation' => '攻撃を確率で回避する',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'マウス',
            'const_effect' => 10,
            'rate_effect' => 0,
            'max' => 100,
            'status_type' => 15,
            'explanation' => '確率でスキルのクールダウンをリセット',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'デジタルミート',
            'const_effect' => 1,
            'rate_effect' => 0,
            'max' => 0,
            'status_type' => 16,
            'explanation' => '20秒ごとに回復アイテムを生成する。15秒経過で消滅する',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'ファイアウォール',
            'const_effect' => 0,
            'rate_effect' => 0.05,
            'max' => 0.5,
            'status_type' => 17,
            'explanation' => '被ダメージを軽減',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'ライフスカベンジャー',
            'const_effect' => 0,
            'rate_effect' => 0.005,
            'max' => 0.05,
            'status_type' => 18,
            'explanation' => '敵撃破時にHPを1%分回復する',
            'rarity' => 2,
        ]);
        Relic::create([
            'name' => 'ラグルーター',
            'const_effect' => 10,
            'rate_effect' => 0,
            'max' => 100,
            'status_type' => 19,
            'explanation' => '攻撃時に確率で２回攻撃する',
            'rarity' => 3,
        ]);
        Relic::create([
            'name' => 'バックアップHDMI',
            'const_effect' => 1,
            'rate_effect' => 0,
            'max' => 0,
            'status_type' => 20,
            'explanation' => '一度復活する　その後破壊される',
            'rarity' => 3,
        ]);
        Relic::create([
            'name' => '識別AI',
            'const_effect' => 0,
            'rate_effect' => 1.0,
            'max' => 0,
            'status_type' => 21,
            'explanation' => '状態異常が付与されている敵に対して追加ダメージを与える',
            'rarity' => 3,
        ]);
        Relic::create([
            'name' => '段ボール人形',
            'const_effect' => 0,
            'rate_effect' => 0.05,
            'max' => 0.50,
            'status_type' => 22,
            'explanation' => '相手の防御を一定値、無視する',
            'rarity' => 4,
        ]);
        Relic::create([
            'name' => 'ロボコア',
            'const_effect' => 1,
            'rate_effect' => 0,
            'max' => 6,
            'status_type' => 23,
            'explanation' => '自身を中心に回転する電気玉を発生する',
            'rarity' => 4,
        ]);
        Relic::create([
            'name' => '違法スクリプト',
            'const_effect' => 2,
            'rate_effect' => 0,
            'max' => 50,
            'status_type' => 24,
            'explanation' => 'ボスを除いて、攻撃時に確率で99999ダメージを与える',
            'rarity' => 4,
        ]);
        Relic::create([
            'name' => 'Caracal.png',
            'const_effect' => 0,
            'rate_effect' => 0,
            'max' => 0,
            'status_type' => 0,
            'explanation' => '効果なし うざい',
            'rarity' => 5,
        ]);
    }
}
