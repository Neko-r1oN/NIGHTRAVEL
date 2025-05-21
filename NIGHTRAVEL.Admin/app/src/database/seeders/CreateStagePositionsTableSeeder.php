<?php

namespace Database\Seeders;

use App\Models\CreateStagePosition;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class CreateStagePositionsTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        CreateStagePosition::create([
            'create_stage_id' => 1,
            'type' => 1,
            'x' => 30,
            'y' => 40
        ]);
    }
}
