<?php

namespace Database\Seeders;

use App\Models\UserRelic;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class UserRelicsTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        UserRelic::create([
            'user_id' => 1,
            'relic_id' => 1,
        ]);
    }
}
