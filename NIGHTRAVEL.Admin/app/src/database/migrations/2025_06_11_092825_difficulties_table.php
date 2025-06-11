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
        Schema::create('difficulties', function (Blueprint $table) {
            $table->id();                                        //idカラム
            $table->string('name');//nameカラム
            $table->integer('conditions');//conditionsカラム
            $table->timestamps();                           //created_atとupdated_at
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('difficulties');
    }
};
