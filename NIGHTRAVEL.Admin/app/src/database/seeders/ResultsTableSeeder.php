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
            'title_id' => 1,
            'weapon_id' => 1,
            'stage_id' => 6,
            'difficulty_id' => 1,
            'is_game_clear' => true,
            'total_score' => 100000,
            'total_kill' => 500,
            'character_level' => 25,
            'alive_time' => 5648,
            'max_given_damage' => 1000,
            'given_damage' => 400,
            'received_damage' => 400,
            'stage_exit_count' => 6,
            'relic_count' => 18,
            'power_up_count' => 25,
            'move_distance' => 158,
            'boss_kill_count' => 20,
            'stage_complete' => 6,
            'play_time' => '01:33:05',
            'dead_count' => 0,
        ]);
    }
}
