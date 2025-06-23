<?php

namespace Database\Seeders;

use App\Models\AchievementStatus;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class AchievementStatusTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        AchievementStatus::create([                   //シーダーを使った初期データの登録
            'user_id' => 1,
            'achievement_id' => 1,
            'progress' => 1,
        ]);
        AchievementStatus::create([                   //シーダーを使った初期データの登録
            'user_id' => 1,
            'achievement_id' => 2,
            'progress' => 5,
        ]);
        AchievementStatus::create([                   //シーダーを使った初期データの登録
            'user_id' => 2,
            'achievement_id' => 1,
            'progress' => 1,
        ]);
        AchievementStatus::create([                   //シーダーを使った初期データの登録
            'user_id' => 3,
            'achievement_id' => 1,
            'progress' => 1,
        ]);
        AchievementStatus::create([                   //シーダーを使った初期データの登録
            'user_id' => 2,
            'achievement_id' => 2,
            'progress' => 1,
        ]);
        AchievementStatus::create([                   //シーダーを使った初期データの登録
            'user_id' => 3,
            'achievement_id' => 3,
            'progress' => 1,
        ]);
    }
}
