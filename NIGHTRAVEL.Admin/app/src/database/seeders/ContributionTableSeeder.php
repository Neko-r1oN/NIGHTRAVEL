<?php

namespace Database\Seeders;

use App\Models\Contribution;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class ContributionTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        Contribution::create([
            'name' => 'テストマスター',
            'explanation' => "称号の説明が入ります",
            'rarity' => 1,
            'conditions' => 10,
            'type' => 1,
        ]);
    }
}
