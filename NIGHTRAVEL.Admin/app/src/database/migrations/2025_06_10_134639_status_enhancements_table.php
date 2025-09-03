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
            $table->float('const_effect1');//const_effect_aカラム(固定値)
            $table->float('rate_effect1');//rate_effect_aカラム(確率)
            $table->float('const_effect2');//const_effect_bカラム(固定値)
            $table->float('rate_effect2');//rate_effect_bカラム(確率)
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
