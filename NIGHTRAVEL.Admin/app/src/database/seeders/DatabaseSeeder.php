<?php

namespace Database\Seeders;

use App\Models\Have;
use App\Models\StatusEnhancement;
use App\Models\User;

// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    /**
     * Seed the application's database.
     */
    public function run(): void
    {
        //シーだーに呼び出し
        $this->call(AccountsTableSeeder::class);
        $this->call(UsersTableSeeder::class);
        $this->call(WeaponsTableSeeder::class);
        $this->call(EnemiesTableSeeder::class);
        $this->call(StagesTableSeeder::class);
        $this->call(ResultsTableSeeder::class);
        $this->call(RelicsTableSeeder::class);
        $this->call(UserRelicsTableSeeder::class);
        $this->call(AchievementsTableSeeder::class);
        $this->call(AchievementStatusTableSeeder::class);
        $this->call(StatusEnhancementTableSeeder::class);
        $this->call(DifficultyTableSeeder::class);
        $this->call(AwardTableSeeder::class);

    }
}
