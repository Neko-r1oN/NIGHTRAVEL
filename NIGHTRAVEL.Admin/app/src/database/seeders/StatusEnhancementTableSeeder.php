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
        StatusEnhancement::create([
            'name' => '攻撃力増加',
            'rarity' => 1,
            'explanation' => '即座に攻撃力が30%増加する',
            'type' => 1,
            'effect' => 1.30,
            'enhancement_type' => '攻撃力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力と防御力増加',
            'rarity' => 2,
            'explanation' => '即座に攻撃力と防御力が15%増加する',
            'type' => 1,
            'effect' => 1.15,
            'enhancement_type' => '攻撃力,防御力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力と移動速度増加',
            'rarity' => 2,
            'explanation' => '即座に攻撃力と移動速度が10%増加する',
            'type' => 1,
            'effect' => 1.10,
            'enhancement_type' => '攻撃力,移動速度',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '攻撃力と防御力増加',
            'rarity' => 2,
            'explanation' => '即座に移動速度と防御力が15%増加する',
            'type' => 1,
            'effect' => 1.15,
            'enhancement_type' => '移動速度,防御力',
            'duplication' => true

        ]);

        StatusEnhancement::create([
            'name' => '防御力増加',
            'rarity' => 1,
            'explanation' => '即座に防御力が15%増加する',
            'type' => 1,
            'effect' => 1.15,
            'enhancement_type' => '防御力',
            'duplication' => true

        ]);
    }
}
