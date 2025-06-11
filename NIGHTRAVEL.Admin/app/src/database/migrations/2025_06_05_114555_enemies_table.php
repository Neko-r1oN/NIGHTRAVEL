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
        Schema::create('enemies', function (Blueprint $table) {
            $table->id();                                        //idカラム
            $table->string('name', 20);             //nameカラム
            $table->float('hp');                        //hpカラム
            $table->float('attack');                     //attackカラム
            $table->float('defence');                     //defenceカラム
            $table->float('move_speed');                     //move_speedカラム
            $table->integer('stage_id');                     //stage_idカラム
            $table->timestamps();                               //created_atとupdated_at

            $table->index('name');                     //nameにインデックス設定
            $table->index('stage_id');                     //stage_idにインデックス設定
            $table->unique('id');                    //idにユニーク制約設定
            $table->unique('name');                    //nameにユニーク制約設定
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('enemies');
    }
};
