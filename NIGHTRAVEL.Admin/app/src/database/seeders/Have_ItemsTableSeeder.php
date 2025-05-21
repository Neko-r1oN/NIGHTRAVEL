<?php

namespace Database\Seeders;

use App\Models\HaveItem;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class Have_ItemsTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        HaveItem::create([
            'user_id' => 1,
            'item_id' => 1,
            'possession' => 10,
        ]);

        HaveItem::create([
            'user_id' => 2,
            'item_id' => 2,
            'possession' => 2,
        ]);

        HaveItem::create([
            'user_id' => 3,
            'item_id' => 3,
            'possession' => 1,
        ]);

        HaveItem::create([
            'user_id' => 3,
            'item_id' => 4,
            'possession' => 1,
        ]);
    }
}
