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
            'explanation' => '即座に攻撃力が1.05倍増加する',
            'type' => 1,
            'effect' => 1.05,
            'duplication' => true,

        ]);
    }
}
