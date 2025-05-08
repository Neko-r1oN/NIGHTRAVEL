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
            Schema::create('accounts', function (Blueprint $table) {
                $table->id();                                 //IDカラム
                $table->string('name', 20);      //nameカラム(20文字)
                $table->string('password', 100); //passwordカラム(100文字)
                $table->timestamps();                         //created_at,updated_at

                $table->index('name');             //nameにindex設定
                $table->unique('name');           //nameにunique制約設定


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
