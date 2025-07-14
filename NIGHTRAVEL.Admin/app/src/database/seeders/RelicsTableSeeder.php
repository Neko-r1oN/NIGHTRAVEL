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
            'name' => 'テストレリック1',
            'effect' => 1.5,
            'explanation' => 'ここにレリック1の効果説明文が入ります',
            'rarity' => 1,
        ]);
        Relic::create([
            'name' => 'テストレリック2',
            'effect' => 3.0,
            'explanation' => 'ここにレリック2の効果説明文',
            'rarity' => 3,
        ]);
        Relic::create([
            'name' => 'テストレリック3',
            'effect' => 2.0,
            'explanation' => 'ここにレリック3の効果説明文',
            'rarity' => 1,
        ]);
    }
}
