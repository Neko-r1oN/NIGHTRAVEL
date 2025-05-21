<?php

namespace Database\Seeders;

use App\Models\UserMail;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class User_MailsTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        UserMail::create([
            'user_id' => 1,
            'mail_id' => 1,
            'open_flag' => false
        ]);

        UserMail::create([
            'user_id' => 1,
            'mail_id' => 3,
            'open_flag' => false
        ]);

        UserMail::create([
            'user_id' => 2,
            'mail_id' => 2,
            'open_flag' => false
        ]);

        UserMail::create([
            'user_id' => 3,
            'mail_id' => 1,
            'open_flag' => true
        ]);

        UserMail::create([
            'user_id' => 3,
            'mail_id' => 2,
            'open_flag' => true
        ]);

        UserMail::create([
            'user_id' => 3,
            'mail_id' => 3,
            'open_flag' => true
        ]);
    }
}
