<?php

namespace Database\Seeders;

use App\Models\Stage;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class StagesTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        Stage::create([
            'name' => 'テストステージ1',
            'descriptive_text' => 'ここにステージ1の説明文が入ります',
        ]);
        Stage::create([
            'name' => 'テストステージ2',
            'descriptive_text' => 'ステージ2の説明文が入ります',
        ]);
        Stage::create([
            'name' => 'テストステージ3',
            'descriptive_text' => 'ここにステージ3の説明文',
        ]);
        Stage::create([
            'name' => 'テストステージ4',
            'descriptive_text' => 'ここにステージ4の説明文',
        ]);
    }
}
