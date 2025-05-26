<?php
/**
 * ステータス強化テーブル
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
        Schema::create('strengthen_statuses', function (Blueprint $table) {
            $table->id();
            $table->integer('user_id');
            $table->string('name',20);
            $table->integer('hp_effect');
            $table->integer('attack_effect');
            $table->float('regene_effect');
            $table->integer('move_effect');
            $table->integer('defence_effect');
            $table->integer('rarity');
            $table->string('explanation',40);
            $table->timestamps();

            //ユニーク
            $table->unique('id');

            //インデックス
            $table->index('user_id');
            $table->index('name');
            $table->index('created_at');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('strengthen_statuses');
    }
};
