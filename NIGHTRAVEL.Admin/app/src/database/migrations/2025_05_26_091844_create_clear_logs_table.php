<?php
/**
 * クリアログテーブル
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
        Schema::create('clear_logs', function (Blueprint $table) {
            $table->id();
            $table->integer('score');           //スコア
            $table->integer('level');           //レベル
            $table->integer('player_id');       //プレイヤーID
            $table->string('relic_name',20);       //レリック名
            $table->timestamps();

            //ユニーク
            $table->unique('id');

            //インデックス
            $table->index('score');
            $table->index('level');
            $table->index('player_id');
            $table->index('created_at');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('clear_logs');
    }
};
