<?php

    namespace Database\Seeders;

    use App\Models\Follow;
    use App\Models\User;

// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
    use Illuminate\Database\Seeder;

    class DatabaseSeeder extends Seeder
    {

        public function run(): void
        {

            $this->call(AccountsTableSeeder::class);
            $this->call(UsersTableSeeder::class);
            
            //$this->call(MailTableSeeder::class);
            //$this->call(OpenMailsTableSeeder::class);
            //$this->call(FollowsTableSeeder::class);

            //基盤データ
            //$this->call(AccountsTableSeeder::class);
            //$this->call(StagesTableSeeder::class);
            //$this->call(ScoreRankingTableSeeder::class);

        }
    }
