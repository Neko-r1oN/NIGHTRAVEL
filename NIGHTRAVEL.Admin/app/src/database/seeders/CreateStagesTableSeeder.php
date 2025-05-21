<?php

namespace Database\Seeders;

use App\Models\CreateStage;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class CreateStagesTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        CreateStage::create([
            'stage_id' => 2
        ]);
    }
}
