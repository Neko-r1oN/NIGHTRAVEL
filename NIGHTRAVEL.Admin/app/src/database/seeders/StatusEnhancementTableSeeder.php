<?php

namespace Database\Seeders;

use App\Models\StatusEnhancement;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class StatusEnhancementTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        /**
         * Run the database seeds.
         */
        StatusEnhancement::create([
            'name' => 'HPアップ(コモン)',
            'rarity' => 1,
            'explanation' => '体力が40増加する',
            'type1' => 1,
            'const_effect1' => 40,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '体力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '防御力アップ(コモン)',
            'rarity' => 1,
            'explanation' => '防御力が8増加する',
            'type1' => 2,
            'const_effect1' => 8,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '防御力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力アップ(コモン)',
            'rarity' => 1,
            'explanation' => '攻撃力が8増加する',
            'type1' => 3,
            'const_effect1' => 8,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '跳躍力アップ(コモン)',
            'rarity' => 1,
            'explanation' => '跳躍力が1増加する',
            'type1' => 4,
            'const_effect1' => 1,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '跳躍力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '移動速度アップ(コモン)',
            'rarity' => 1,
            'explanation' => '移動速度が1増加する',
            'type1' => 5,
            'const_effect1' => 1,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '移動速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃速度アップ(コモン)',
            'rarity' => 1,
            'explanation' => '攻撃速度が微かに増加する',
            'type1' => 6,
            'const_effect1' => 0.02,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '自動回復の倍率増加(コモン)',
            'rarity' => 1,
            'explanation' => '自動回復の倍率が0.02増加する',
            'type1' => 7,
            'const_effect1' => 0.02,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '自動回復',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => 'HPアップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '体力が60増加する',
            'type1' => 1,
            'const_effect1' => 60,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '体力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '防御力アップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '防御力が20増加する',
            'type1' => 2,
            'const_effect1' => 20,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '防御力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力アップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '攻撃力が20増加する',
            'type1' => 3,
            'const_effect1' => 20,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '跳躍力アップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '即座に跳躍力を3増加させる',
            'type1' => 4,
            'const_effect1' => 3,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '跳躍力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '移動速度アップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '即座に移動速度を3増加させる',
            'type1' => 5,
            'const_effect1' => 3,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '移動速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃速度アップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '攻撃速度が少し増加する',
            'type1' => 6,
            'const_effect1' => 0.05,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '自動回復の倍率アップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '自動回復の倍率を0.04増加する',
            'type1' => 7,
            'const_effect1' => 0.04,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '自動回復',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => 'HPアップ(レア)',
            'rarity' => 3,
            'explanation' => '体力が160増加する',
            'type1' => 1,
            'const_effect1' => 160,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '体力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '防御力アップ(レア)',
            'rarity' => 3,
            'explanation' => '防御力が80増加する',
            'type1' => 2,
            'const_effect1' => 80,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '防御力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力アップ(レア)',
            'rarity' => 3,
            'explanation' => '攻撃力が80増加する',
            'type1' => 3,
            'const_effect1' => 80,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '跳躍力アップ(レア)',
            'rarity' => 3,
            'explanation' => '跳躍力が6増加する',
            'type1' => 4,
            'const_effect1' => 6,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '跳躍力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '移動速度アップ(レア)',
            'rarity' => 3,
            'explanation' => '移動速度を6増加する',
            'type1' => 5,
            'const_effect1' => 6,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃速度',
            'duplication' => true

        ]);


        StatusEnhancement::create([
            'name' => '攻撃速度アップ(レア)',
            'rarity' => 3,
            'explanation' => '攻撃速度が増加',
            'type1' => 6,
            'const_effect1' => 0.15,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '自動回復の倍率アップ(レア)',
            'rarity' => 3,
            'explanation' => '即座に自動回復の倍率を0.06増加',
            'type1' => 7,
            'const_effect1' => 0.06,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '自動回復',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '体力アップ、自動回復の倍率ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '即座にHPを30%増加、自動回復の倍率を5%減少する',
            'type1' => 1,
            'const_effect1' => 0,
            'rate_effect1' => 1.3,
            'type2' => 7,
            'const_effect2' => 0,
            'rate_effect2' => 0.95,
            'enhancement_type' => '体力、自動回復',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '防御力アップ、攻撃力ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '即座に防御力を20%増加、攻撃力を10%減少',
            'type1' => 2,
            'const_effect1' => 0,
            'rate_effect1' => 1.20,
            'type2' => 3,
            'const_effect2' => 0,
            'rate_effect2' => 0.90,
            'enhancement_type' => '防御力、攻撃力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力アップ、移動速度ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '即座に攻撃力が20%増加、移動速度5%減少する',
            'type1' => 3,
            'const_effect1' => 0,
            'rate_effect1' => 1.20,
            'type2' => 5,
            'const_effect2' => 0,
            'rate_effect2' => 0.95,
            'enhancement_type' => '攻撃力、移動速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '跳躍力アップ、攻撃速度ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '即座に跳躍力が10%増加、攻撃速度が5%減少する',
            'type1' => 4,
            'const_effect1' => 0,
            'rate_effect1' => 1.10,
            'type2' => 6,
            'const_effect2' => 0,
            'rate_effect2' => 0.95,
            'enhancement_type' => '跳躍力、攻撃速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '移動速度アップ、体力ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '即座に移動速度が10%増加、体力が10%減少する',
            'type1' => 5,
            'const_effect1' => 0,
            'rate_effect1' => 1.10,
            'type2' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 0.90,
            'enhancement_type' => '移動速度、体力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃速度アップ、跳躍力ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '攻撃速度が大きく増加、跳躍力が5%減少する',
            'type1' => 6,
            'const_effect1' => 0.3,
            'rate_effect1' => 1.0,
            'type2' => 4,
            'const_effect2' => 0,
            'rate_effect2' => 0.95,
            'enhancement_type' => '攻撃速度、跳躍力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '自動回復の倍率アップ、防御力ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '自動回復の倍率を10%増加、防御力が10%減少する',
            'type1' => 7,
            'const_effect1' => 0,
            'rate_effect1' => 1.10,
            'type2' => 2,
            'const_effect2' => 0,
            'rate_effect2' => 0.90,
            'enhancement_type' => '跳躍力、攻撃速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '体力アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '体力が50%増加する',
            'type1' => 1,
            'const_effect1' => 0,
            'rate_effect1' => 1.5,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '体力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '防御力アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '防御力が30%増加する',
            'type1' => 2,
            'const_effect1' => 0,
            'rate_effect1' => 1.3,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '防御力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '攻撃力が30%増加する',
            'type1' => 3,
            'const_effect1' => 0,
            'rate_effect1' => 1.3,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '攻撃力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '跳躍力アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '跳躍力が20%増加する',
            'type1' => 4,
            'const_effect1' => 0,
            'rate_effect1' => 1.2,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '跳躍力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '移動速度アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '移動速度が20%増加する',
            'type1' => 5,
            'const_effect1' => 0,
            'rate_effect1' => 1.2,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '移動速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃速度アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '攻撃速度がとても大きく増加',
            'type1' => 6,
            'const_effect1' => 0.5,
            'rate_effect1' => 1,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '攻撃速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '自動回復の倍率アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '自動回復の倍率が20%増加',
            'type1' => 7,
            'const_effect1' => 0,
            'rate_effect1' => 1.2,
            'type2' => 0,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '自動回復',
            'duplication' => true
        ]);
    }
}
