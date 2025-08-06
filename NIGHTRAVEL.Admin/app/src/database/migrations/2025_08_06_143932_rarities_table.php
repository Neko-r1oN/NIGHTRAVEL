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
        Schema::create('rarities', function (Blueprint $table) {
            $table->id();                                        //idカラム
            $table->string('rarity_name');//rarity_nameカラム
            $table->integer('emission_rate');//emission_rateカラム
            $table->timestamps();                           //created_atとupdated_at
            $table->unique('id');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('rarities');
    }
};
