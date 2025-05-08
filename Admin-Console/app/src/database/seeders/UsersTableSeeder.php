<?php

    namespace Database\Seeders;

    use App\Models\User;
    use Illuminate\Database\Console\Seeds\WithoutModelEvents;
    use Illuminate\Database\Seeder;

    class UsersTableSeeder extends Seeder
    {
        /**
         * Run the database seeds.
         */
        public function run(): void
        {

            User::create([

                'id' => 1,
                'name' => 'test1',
                'token' => uniqid(),

            ]);
            User::create([
                'id' => 2,
                'name' => 'test2',
                'token' => uniqid(),

            ]);
            User::create([
                'id' => 3,
                'name' => 'test3',
                'token' => uniqid(),

            ]);
            User::create([

                'id' => 4,
                'name' => 'test4',
                'token' => uniqid(),

            ]);
            User::create([
                'id' => 5,
                'name' => 'test5',
                'token' => uniqid(),

            ]);
            User::create([
                'id' => 6,
                'name' => 'test6',
                'token' => uniqid(),

            ]);


        }

    }
