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
            'explanation' => '即座に体力が20増加する',
            'type' => 1,
            'const_effect1' => 20,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '体力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '防御力アップ(コモン)',
            'rarity' => 1,
            'explanation' => '即座に防御力が5増加する',
            'type' => 2,
            'const_effect1' => 5,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '防御力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力アップ(コモン)',
            'rarity' => 1,
            'explanation' => '即座に攻撃力が5増加する',
            'type' => 3,
            'const_effect1' => 5,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '跳躍力アップ(コモン)',
            'rarity' => 1,
            'explanation' => '即座に跳躍力が1増加する',
            'type' => 4,
            'const_effect1' => 1,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '跳躍力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '移動速度アップ(コモン)',
            'rarity' => 1,
            'explanation' => '即座に移動速度が1増加する',
            'type' => 5,
            'const_effect1' => 1,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '移動速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃速度アップ(コモン)',
            'rarity' => 1,
            'explanation' => '即座に攻撃速度が0.05増加する',
            'type' => 7,
            'const_effect1' => 0.05,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '自動回復の倍率増加(コモン)',
            'rarity' => 1,
            'explanation' => '即座に自動回復の倍率が0.01増加する',
            'type' => 8,
            'const_effect1' => 0.01,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '自動回復',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => 'HPアップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '即座に体力が60増加する',
            'type' => 1,
            'const_effect1' => 60,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '体力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '防御力アップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '即座に防御力が20増加する',
            'type' => 2,
            'const_effect1' => 20,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '防御力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力アップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '即座に攻撃力が20増加する',
            'type' => 3,
            'const_effect1' => 20,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '跳躍力アップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '即座に跳躍力を3増加させる',
            'type' => 4,
            'const_effect1' => 3,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '跳躍力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '移動速度アップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '即座に移動速度を3増加させる',
            'type' => 5,
            'const_effect1' => 3,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '移動速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃速度アップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '即座に攻撃速度が0.1増加する',
            'type' => 7,
            'const_effect1' => 0.1,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '自動回復の倍率アップ(アンコモン)',
            'rarity' => 2,
            'explanation' => '自動回復の倍率を0.03増加する',
            'type' => 8,
            'const_effect1' => 0.03,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '自動回復',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => 'HPアップ(レア)',
            'rarity' => 3,
            'explanation' => '即座に体力が180増加する',
            'type' => 1,
            'const_effect1' => 180,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '体力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '防御力アップ(レア)',
            'rarity' => 3,
            'explanation' => '即座に防御力が90増加する',
            'type' => 2,
            'const_effect1' => 90,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '防御力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力アップ(レア)',
            'rarity' => 3,
            'explanation' => '即座に攻撃力が90増加する',
            'type' => 3,
            'const_effect1' => 90,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '跳躍力アップ(レア)',
            'rarity' => 3,
            'explanation' => '即座に跳躍力が6増加する',
            'type' => 4,
            'const_effect1' => 6,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '跳躍力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '移動速度アップ(レア)',
            'rarity' => 3,
            'explanation' => '即座に移動速度を6増加する',
            'type' => 5,
            'const_effect1' => 6,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃速度',
            'duplication' => true

        ]);


        StatusEnhancement::create([
            'name' => '攻撃速度アップ(レア)',
            'rarity' => 3,
            'explanation' => '即座に攻撃速度が0.5増加',
            'type' => 7,
            'const_effect1' => 0.5,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '攻撃速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '自動回復の倍率アップ(レア)',
            'rarity' => 3,
            'explanation' => '即座に自動回復の倍率を0.05増加',
            'type' => 8,
            'const_effect1' => 0.05,
            'rate_effect1' => 1,
            'const_effect2' => 0,
            'rate_effect2' => 1,
            'enhancement_type' => '自動回復',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '体力アップ、自動回復の倍率ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '即座にHPを30%増加、自動回復の倍率を5%減少する',
            'type' => 1,
            'const_effect1' => 0,
            'rate_effect1' => 1.3,
            'const_effect2' => 0,
            'rate_effect2' => 0.95,
            'enhancement_type' => '体力、自動回復',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '防御力アップ、攻撃力ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '即座に防御力を20%増加、攻撃力を10%減少',
            'type' => 2,
            'const_effect1' => 0,
            'rate_effect1' => 1.20,
            'const_effect2' => 0,
            'rate_effect2' => 0.90,
            'enhancement_type' => '防御力、攻撃力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力アップ、移動速度ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '即座に攻撃力が20%増加、移動速度5%減少する',
            'type' => 3,
            'const_effect1' => 0,
            'rate_effect1' => 1.20,
            'const_effect2' => 0,
            'rate_effect2' => 0.95,
            'enhancement_type' => '攻撃力、移動速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '跳躍力アップ、攻撃速度ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '即座に跳躍力が10%増加、攻撃速度が5%減少する',
            'type' => 4,
            'const_effect1' => 0,
            'rate_effect1' => 1.10,
            'const_effect2' => 0,
            'rate_effect2' => 0.95,
            'enhancement_type' => '跳躍力、攻撃速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '移動速度アップ、体力ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '即座に移動速度が10%増加、体力が10%減少する',
            'type' => 5,
            'const_effect1' => 0,
            'rate_effect1' => 1.10,
            'const_effect2' => 0,
            'rate_effect2' => 0.90,
            'enhancement_type' => '移動速度、体力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃速度アップ、跳躍力ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '即座に攻撃速度が10%増加、跳躍力が5%減少する',
            'type' => 7,
            'const_effect1' => 0,
            'rate_effect1' => 1.10,
            'const_effect2' => 0,
            'rate_effect2' => 0.95,
            'enhancement_type' => '攻撃速度、跳躍力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '自動回復の倍率アップ、防御力ダウン(ユニーク)',
            'rarity' => 4,
            'explanation' => '即座に自動回復の倍率を10%増加、防御力が10%減少する',
            'type' => 8,
            'const_effect1' => 0,
            'rate_effect1' => 1.10,
            'const_effect2' => 0,
            'rate_effect2' => 0.90,
            'enhancement_type' => '跳躍力、攻撃速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '体力アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '即座に体力が50%増加',
            'type' => 1,
            'const_effect1' => 0,
            'rate_effect1' => 1.5,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '体力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '防御力アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '即座に防御力が30%増加',
            'type' => 2,
            'const_effect1' => 0,
            'rate_effect1' => 1.3,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '防御力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '即座に攻撃力が30%増加',
            'type' => 3,
            'const_effect1' => 0,
            'rate_effect1' => 1.3,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '攻撃力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '跳躍力アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '即座に跳躍力が20%増加',
            'type' => 4,
            'const_effect1' => 0,
            'rate_effect1' => 1.2,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '跳躍力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '移動速度アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '即座に移動速度が20%増加',
            'type' => 5,
            'const_effect1' => 0,
            'rate_effect1' => 1.2,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '移動速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃速度アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '即座に攻撃速度が20%増加',
            'type' => 7,
            'const_effect1' => 0,
            'rate_effect1' => 1.2,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '攻撃速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '自動回復の倍率アップ(レジェンド)',
            'rarity' => 5,
            'explanation' => '即座に自動回復の倍率が20%増加',
            'type' => 8,
            'const_effect1' => 0,
            'rate_effect1' => 1.2,
            'const_effect2' => 0,
            'rate_effect2' => 1.0,
            'enhancement_type' => '自動回復',
            'duplication' => true

        ]);

    }
}
