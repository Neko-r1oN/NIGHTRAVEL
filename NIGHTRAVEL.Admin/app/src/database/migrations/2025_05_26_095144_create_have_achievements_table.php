<?php
/**
 * 所持アチーブメントテーブル
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
        Schema::create('have_achievements', function (Blueprint $table) {
            $table->id();
            $table->integer('player_id');           //プレイヤーID
            $table->integer('achievement_id');      //アチーブメントID
            $table->time('unlocked_at');            //アンロック
            $table->timestamps();

            //ユニーク
            $table->unique('id');

            //インデックス
            $table->index('player_id');
            $table->index('achievement_id');
            $table->index('created_at');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('have_achievements');
    }
};
