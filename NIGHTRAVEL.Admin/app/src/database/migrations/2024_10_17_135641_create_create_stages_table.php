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
        Schema::create('create_stages', function (Blueprint $table) {
            $table->id();                               //クリエイトステージID
            $table->integer('stage_id');         //クリエイトしたステージのID
            $table->timestamps();                       //登録日時
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('create_stages');
    }
};
