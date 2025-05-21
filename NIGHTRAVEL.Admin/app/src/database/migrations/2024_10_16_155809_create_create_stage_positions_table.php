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
        Schema::create('create_stage_positions', function (Blueprint $table) {
            $table->id();                                   //ID
            $table->integer('create_stage_id');      //クリエイトステージのID
            $table->integer('type');                 //種別
            $table->float('x');                      //X座標
            $table->float('y');                     //Y座標
            $table->timestamps();                           //登録日時

            //ユニーク制約設定
            $table->unique(['create_stage_id', 'type']);
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('create_stage_positions');
    }
};
