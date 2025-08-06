<?php

namespace Database\Seeders;

use App\Models\RelicRarity;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class RaritiesTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        RelicRarity::create([
            'rarity_name' => 'コモン',   //rarity_nameカラム
            'emission_rate' => 45, //emission_rateカラム
        ]);
        RelicRarity::create([
            'rarity_name' => 'アンコモン',   //rarity_nameカラム
            'emission_rate' => 35, //emission_rateカラム
        ]);
        RelicRarity::create([
            'rarity_name' => 'レア',   //rarity_nameカラム
            'emission_rate' => 12, //emission_rateカラム
        ]);
        RelicRarity::create([
            'rarity_name' => 'ユニーク',   //rarity_nameカラム
            'emission_rate' => 6, //emission_rateカラム
        ]);
        RelicRarity::create([
            'rarity_name' => 'レジェンド',   //rarity_nameカラム
            'emission_rate' => 2, //emission_rateカラム
        ]);
        RelicRarity::create([
            'rarity_name' => 'ボス',   //rarity_nameカラム
            'emission_rate' => 45, //emission_rateカラム
        ]);
    }
}
