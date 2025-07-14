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
        Schema::create('achievement_statuses', function (Blueprint $table) {
            $table->id();                                        //idカラム
            $table->integer('user_id');//user_idカラム
            $table->integer('achievement_id');//achievement_idカラム
            $table->integer('progress');//progressカラム
            $table->timestamps();                           //created_atとupdated_at

            $table->index('user_id');//user_idにユニーク制約設定
            $table->index('achievement_id');//achievement_idにユニーク制約設定
            $table->unique('id');                    //idにユニーク制約設定
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('achievement_statuses');
    }
};
