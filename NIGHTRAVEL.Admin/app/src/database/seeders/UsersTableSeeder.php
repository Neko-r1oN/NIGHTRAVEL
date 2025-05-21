<?php

namespace Database\Seeders;

use App\Models\User;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class UsersTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        //シーだーでファクトリーを呼び出す。数値は生成するレコード数
        User::factory(100)->create();

        /*
        User::create([
            'name' => 'soruteiiiiiiiiii',
            'level' => 30,
            'experience_point' => 60,
            'life' => 50
        ]);
        User::create([
            'name' => 'miyake',
            'level' => 10,
            'experience_point' => 99,
            'life' => 75
        ]);
        User::create([
            'name' => '汐',
            'level' => 90,
            'experience_point' => 1,
            'life' => 100
        ]);
        */
    }
}
