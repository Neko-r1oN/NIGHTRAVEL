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
        Schema::create('achievements', function (Blueprint $table) {
            $table->id();                                        //idカラム
            $table->string('condition', 100);//conditionカラム
            $table->string('name', 20);//nameカラム
            $table->integer('condition_complete');//condition_completeカラム
            $table->integer('type');//typeカラム
            $table->timestamps();                               //created_atとupdated_at

            $table->unique('id');                    //idにユニーク制約設定
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('achievements');
    }
};
