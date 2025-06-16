<?php

namespace Database\Seeders;

use App\Models\Difficulty;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class DifficultyTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        Difficulty::create([
            'name' => 'イージー',
            'conditions' => 1
        ]);
        Difficulty::create([
            'name' => 'ノーマル',
            'conditions' => 2
        ]);
        Difficulty::create([
            'name' => 'ハード',
            'conditions' => 3
        ]);
    }
}
