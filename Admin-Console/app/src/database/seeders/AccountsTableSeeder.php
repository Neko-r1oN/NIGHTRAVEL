<?php

    namespace Database\Seeders;

    use App\Models\Account;
    use Illuminate\Database\Console\Seeds\WithoutModelEvents;
    use Illuminate\Database\Seeder;
    use Illuminate\Support\Facades\Hash;

    class AccountsTableSeeder extends Seeder
    {

        public function run(): void
        {
            // User::factory(10)->create();

            Account::create([

                'name' => 'jobi',
                'password' => Hash::make('jobi'),
            ]);
            Account::create([

                'name' => 'test',
                'password' => Hash::make('test'),
            ]);
            Account::create([

                'name' => 'r1oN',
                'password' => Hash::make('r1oN'),
            ]);

        }
    }
