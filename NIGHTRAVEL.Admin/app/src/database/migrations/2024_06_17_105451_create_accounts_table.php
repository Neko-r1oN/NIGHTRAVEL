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
        //ターブルのカラム構成を指定
        Schema::create('accounts', function (Blueprint $table) {
            $table->id();                                      //idカラム
            $table->string('account_name', 20);          //nameカラム（20文字）
            $table->string('password');                 //パスワード
            $table->timestamps();                              //created_atとupdated_at

            //$table->index('name');                     //nameにインデックス設定
            $table->unique('id');                    //idにユニーク制約設定
            $table->unique('account_name');                    //nameにユニーク制約設定
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('accounts');
    }
};
