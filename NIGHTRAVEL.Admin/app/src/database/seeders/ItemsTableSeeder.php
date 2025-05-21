<?php

namespace Database\Seeders;

use App\Models\Item;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class ItemsTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        Item::create([
            'name' => '回復薬グレート',
            'type' => '消耗品',
            'effect' => 60,
            'info' => 'ライフを回復する。回復薬より回復量が多い。'
        ]);

        Item::create([
            'name' => '秘薬',
            'type' => '消耗品',
            'effect' => 100,
            'info' => '体力を全回復する。'
        ]);

        Item::create([
            'name' => 'エンプレスグリーヴ',
            'type' => '装備品',
            'effect' => 80,
            'info' => '炎妃龍の青き炎を封じ込めた脚用装備。その内には炎妃龍の青き炎が封じられている。'
        ]);

        Item::create([
            'name' => 'ドラゴンヘッド',
            'type' => '装備品',
            'effect' => 100,
            'info' => '黒龍の唸りを思い起こさせる頭用装備。いもしない黒龍の視線に怯え狂死した使用者も。'
        ]);
    }
}
