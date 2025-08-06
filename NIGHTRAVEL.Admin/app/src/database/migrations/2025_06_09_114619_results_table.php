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
        Schema::create('results', function (Blueprint $table) {
            $table->id();                                        //idカラム
            $table->integer('user_id');             //user_idカラム
            $table->integer('Character_id');             //Character_idカラム
            $table->integer('difficulty_id');             //difficulty_idカラム
            $table->integer('relic_count');             //relic_countカラム
            $table->integer('stage_complete');             //stage_completeカラム
            $table->integer('character_level');             //character_levelカラム
            $table->time('alive_time');             //alive_timeカラム
            $table->integer('total_kill');             //total_killカラム
            $table->integer('given_damage');             //given_damageカラム
            $table->integer('received_damage');             //received_damageカラム
            $table->integer('total_get_item');             //total_get_itemカラム
            $table->integer('total_launch_device');             //total_launch_deviceカラム
            $table->timestamps();                               //created_atとupdated_at

            $table->unique('id');                    //idにユニーク制約設定
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('results');
    }
};
