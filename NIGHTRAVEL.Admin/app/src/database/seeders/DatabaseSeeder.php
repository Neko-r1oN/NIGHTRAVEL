<?php

namespace Database\Seeders;

use App\Models\Have;
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

    }
}
