<?php

namespace Database\Seeders;

use App\Models\Result;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class ResultsTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        Result::create([
            'user_id' => 1,
            'Character_id' => 1,
            'difficulty_id' => 1,
            'relic_count' => 6,
            'stage_complete' => 1,
            'character_level' => 25,
            'alive_time' => 5648,
            'total_kill' => 500,
            'given_damage' => 400,
            'received_damage' => 400,
            'total_get_item' => 20,
            'total_launch_device' => 10,
        ]);
    }
}
