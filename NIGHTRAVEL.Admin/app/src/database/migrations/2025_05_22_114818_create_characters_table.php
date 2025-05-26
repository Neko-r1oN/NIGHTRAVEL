<?php
/**
 * キャラクターテーブル
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
        Schema::create('characters', function (Blueprint $table) {
            $table->id();
            $table->string('name',20);          //名前
            $table->integer('role_id');               //ロールID
            $table->integer('hp');                      //HP
            $table->integer('attack_power');            //攻撃力
            $table->integer('move_speed');              //移動速度
            $table->float('regene_speed');            //自動回復速度
            $table->integer('defence_power');           //防御力
            $table->timestamps();

            //ユニーク
            $table->unique('id');
            $table->unique('name');

            //インデックス
            $table->index('role_id');
            $table->index('timestamps');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('characters');
    }
};
