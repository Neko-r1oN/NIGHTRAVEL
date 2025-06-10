<?php

namespace Database\Seeders;

use App\Models\Achievement;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class AchievementsTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        Achievement::create([                   //シーダーを使った初期データの登録
            'condition' => '条件を達成する',
            'name' => '実績のテスト',
            'condition_complete' => 1,
            'type' => 1,

        ]);
    }
}
