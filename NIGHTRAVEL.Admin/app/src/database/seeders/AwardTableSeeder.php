<?php

namespace Database\Seeders;

use App\Models\Award;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class AwardTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        Award::create([
            'name' => 'テストマスター',
            'explanation' => "称号の説明が入ります",
            'rarity' => 1,
            'conditions' => 10,
            'type' => 1,
        ]);
    }
}
