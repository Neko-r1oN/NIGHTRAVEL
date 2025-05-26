<?php
/**
 * エネミーテーブル
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
        Schema::create('enemies', function (Blueprint $table) {
            $table->id();
            $table->integer('stage_id');            //ステージID
            $table->string('name',20);        //名前
            $table->float('hp');                  //HP
            $table->float('attack');              //攻撃力
            $table->float('move_speed');          //移動速度
            $table->integer('type');                //属性
            $table->timestamps();

            //ユニーク
            $table->unique('name');

            //インデックス
            $table->index('stage_id');
            $table->index('name');
            $table->index('timestamps');

        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('enemies');
    }
};
