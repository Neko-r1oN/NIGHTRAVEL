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
            $table->integer('title_id');             //title_idカラム
            $table->integer('weapon_id');             //weapon_idカラム
            $table->integer('stage_id');             //user_idカラム
            $table->integer('difficulty_id');             //difficulty_idカラム
            $table->boolean('is_game_clear');             //is_game_clearカラム
            $table->integer('total_score');             //total_scoreカラム
            $table->integer('total_kill');             //total_killカラム
            $table->integer('character_level');             //character_levelカラム
            $table->time('alive_time');             //alive_timeカラム
            $table->integer('max_given_damage');             //max_given_damageカラム
            $table->integer('given_damage');             //given_damageカラム
            $table->integer('received_damage');             //received_damageカラム
            $table->integer('stage_exit_count');             //stage_exit_countカラム
            $table->integer('relic_count');             //relic_countカラム
            $table->integer('power_up_count');             //power_up_countカラム
            $table->float('move_distance');             //move_distanceカラム
            $table->integer('boss_kill_count');             //boss_kill_countカラム
            $table->integer('stage_complete');             //stage_completeカラム
            $table->time('play_time');             //play_timeカラム
            $table->integer('dead_count');             //dead_countカラム
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
