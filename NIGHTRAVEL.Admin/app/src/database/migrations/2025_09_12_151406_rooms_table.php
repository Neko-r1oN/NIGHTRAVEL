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
        Schema::create('rooms', function (Blueprint $table) {
            $table->id();                                        //idカラム
            $table->string('roomName');//roomNameカラム
            $table->string('userName');//userNameカラム
            $table->string('password');//passwordカラム
            $table->boolean('is_started');//is_startedカラム
            $table->timestamps();                           //created_atとupdated_at
            $table->unique('id');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('rooms');
    }
};
