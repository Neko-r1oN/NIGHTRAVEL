<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration {
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        //テーブルのカラム構成を指定
        Schema::create('relics', function (Blueprint $table) {
            $table->id();                                        //idカラム
            $table->string('name', 20);             //nameカラム
            $table->float('effect');                     //effectカラム
            $table->string('explanation');               //explanationカラム
            $table->integer('rarity');               //rarityカラム
            $table->timestamps();                               //created_atとupdated_at

            $table->unique('id');                    //idにユニーク制約設定
            $table->index('rarity');                    //nameにユニーク制約設定
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('relics');
    }
};
