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
            'name' => 'sample1'
        ]);
        User::create([
            'name' => 'sample2'
        ]);
        User::create([
            'name' => 'sample3'
        ]);
        User::create([
            'name' => 'sample4'
        ]);
    }
}
