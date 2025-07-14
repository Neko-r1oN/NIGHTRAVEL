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
        Schema::create('stages', function (Blueprint $table) {
            $table->id();                                        //idカラム
            $table->string('name', 20);             //nameカラム
            $table->string('descriptive_text');                     //descriptive_textカラム
            $table->timestamps();                               //created_atとupdated_at

            $table->unique('id');                    //idにユニーク制約設定
            $table->unique('name');                    //nameにユニーク制約設定
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('stages');
    }
};
