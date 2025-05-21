<?php

namespace Database\Seeders;

use App\Models\Account;
use App\Models\FollowLists;
use App\Models\HaveItem;
use App\Models\User;

// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use App\Models\User_Mail;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    /**
     * Seed the application's database.
     */
    public function run(): void
    {
        $this->call(AccountsTableSeeder::class);
        $this->call(UsersTableSeeder::class);
        $this->call(ItemsTableSeeder::class);
        $this->call(Have_ItemsTableSeeder::class);
        $this->call(MailsTableSeeder::class);
        $this->call(User_MailsTableSeeder::class);
        $this->call(FollowListsTableSeeder::class);
        $this->call(CreateStagesTableSeeder::class);
        $this->call(CreateStagePositionsTableSeeder::class);
    }
}
