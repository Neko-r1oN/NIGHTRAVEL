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
        Schema::create('status_enhancements', function (Blueprint $table) {
            $table->id();                                        //idカラム
            $table->string('name');//nameカラム
            $table->integer('rarity');//rarityカラム
            $table->string('explanation');//explanationカラム
            $table->integer('type');//typeカラム
            $table->float('effect');//effectカラム
            $table->string('enhancement_type'); //enhancement_typeカラム
            $table->boolean('duplication');//duplicationカラム
            $table->timestamps();                           //created_atとupdated_at

            $table->index('name');
            $table->unique('id');                    //idにユニーク制約設定

        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('status_enhancements');
    }
};
