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
            'name' => 'テストステージ',
            'effect' => 1.5,
            'explanation' => 'ここにレリックの効果説明文が入ります',
            'rarity' => 1,
        ]);
    }
}
