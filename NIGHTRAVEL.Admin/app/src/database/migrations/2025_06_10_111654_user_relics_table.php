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
        Schema::create('user_relics', function (Blueprint $table) {
            $table->id();                                        //idカラム
            $table->integer('user_id');//nameカラム
            $table->integer('relic_id');//effectカラム
            $table->timestamps();                               //created_atとupdated_at

            $table->unique('id');                    //idにユニーク制約設定
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('user_relics');
    }
};
