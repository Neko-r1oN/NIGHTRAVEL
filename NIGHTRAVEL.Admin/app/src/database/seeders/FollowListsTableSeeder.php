<?php

namespace Database\Seeders;

use App\Models\FollowLists;
use Illuminate\Database\Seeder;

class FollowListsTableSeeder extends Seeder
{
    public function run(): void
    {
        FollowLists::create([
            'user_id' => 1,
            'follow_user' => 2
        ]);

        FollowLists::create([
            'user_id' => 2,
            'follow_user' => 1
        ]);
    }
}
