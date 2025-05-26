<?php
/**
 * レリックテーブル
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
        Schema::create('relics', function (Blueprint $table) {
            $table->id();
            $table->string('name',20);          //名前
            $table->float('effect');                  //効果量
            $table->string('explanation',40);   //効果説明
            $table->integer('rarity');                //レア度
            $table->timestamps();

            //ユニーク
            $table->unique('id');
            $table->unique('name');

            //インデックス
            $table->unique('rarity');
            $table->unique('created_at');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('relics');
    }
};
