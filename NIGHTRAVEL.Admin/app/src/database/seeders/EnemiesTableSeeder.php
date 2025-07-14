<?php

namespace Database\Seeders;

use App\Models\Enemy;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class EnemiesTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        Enemy::create([
            'name' => 'ドローン',
            'stage_id' => '1,2,4',
            'hp' => 10.0,
            'attack' => 5.0,
            'defence' => 1.0,
            'move_speed' => 20.0
        ]);

        Enemy::create([
            'name' => 'ロボ警備員',
            'stage_id' => '1,2',
            'hp' => 20.0,
            'attack' => 8.0,
            'defence' => 1.0,
            'move_speed' => 10.0
        ]);

        Enemy::create([
            'name' => 'ロボドッグ',
            'stage_id' => '1,3',
            'hp' => 5.0,
            'attack' => 8.0,
            'defence' => 1.0,
            'move_speed' => 30.0
        ]);
    }
}
