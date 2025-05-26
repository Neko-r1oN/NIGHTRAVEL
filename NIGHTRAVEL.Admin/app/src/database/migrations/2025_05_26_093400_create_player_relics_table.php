<?php
/**
 * プレイヤーレリックテーブル
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
        Schema::create('player_relics', function (Blueprint $table) {
            $table->id();
            $table->integer('player_id');           //プレイヤーID
            $table->integer('relic_id');            //レリックID
            $table->integer('quantity');            //所持数
            $table->timestamps();

            //ユニーク
            $table->unique('id');

            //インデックス
            $table->index('quantity');
            $table->index('timestamps');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('player_relics');
    }
};
