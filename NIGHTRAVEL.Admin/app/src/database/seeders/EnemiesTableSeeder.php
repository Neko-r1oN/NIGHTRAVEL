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
            'name' => 'Hakonov_TypeA',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '1',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Hakonov_TypeB',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '1',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Hakonov_TypeC',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '1',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Signabot',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '1',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Delibot',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '1',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Vendbot',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '1',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Boxgeist',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '1',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Drone',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '1',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Guardbot',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '1',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'CyberDog',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '2',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'MissileMachine',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '2',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'CyberDog_ByWorm',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '2',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Drone_ByWorm',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '2',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'FullMetalWorm',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '2',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'MetalBody',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '2',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Carcass',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '3',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Node_Core',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '3',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Slade',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '3',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Blaze',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '3',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'Valcus',
            'isboss' => false,
            'hp' => 100,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '3',
            'exp' => 5
        ]);
        Enemy::create([
            'name' => 'ValksCodecrysta',
            'isboss' => false,
            'hp' => 500,
            'defence' => 15,
            'power' => 35,
            'jump_power' => 7.0,
            'move_speed' => 8.0,
            'attack_speed_factor' => 1.0,
            'stage_id' => '4',
            'exp' => 5
        ]);
    }
}
