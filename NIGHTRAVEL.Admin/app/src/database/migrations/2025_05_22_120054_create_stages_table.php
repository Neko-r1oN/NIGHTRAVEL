<?php
/**
 * ステージテーブル
 */
use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('stages', function (Blueprint $table) {
            $table->id();
            $table->string('name',20);                              //名前
            $table->boolean('stage_clear')->default(false);          //クリアor失敗
            $table->integer('challenge_level');                            //ステージ難易度
            $table->timestamps();

            //ユニーク
            $table->unique('id');

            //インデックス
            $table->index('name');
            $table->index('stage_clear');
            $table->index('challenge_level');
            $table->index('timestamps');
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
