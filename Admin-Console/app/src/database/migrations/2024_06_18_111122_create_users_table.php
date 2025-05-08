<?php

    use Illuminate\Database\Migrations\Migration;
    use Illuminate\Database\Schema\Blueprint;
    use Illuminate\Support\Facades\Schema;

    return new class extends Migration {
        /**
         * 'id' => 1,
         * 'name' => 'r1oN',
         * 'level' => 29,
         * 'exp' => 290,
         * 'life' => 1000,
         * Run the migrations.
         */
        public function up(): void
        {
            Schema::create('users', function (Blueprint $table) {
                $table->id();                                    //IDカラム
                $table->string('name', 10);         //nameカラム(上限10文字)
                $table->integer('user_id');
                $table->string('token');                  //tokenカラム
                $table->timestamps();                            //生成日時,更新日時

                $table->index('id');                //idにindex設定(検索用)
                $table->index('name');              //nameにindex設定(検索用)
                $table->index('user_id');           //user_idにindex設定(検索用)

                $table->unique('user_id');          //ユーザーidにunique制約設定
            });
        }

        /**
         * Reverse the migrations.
         */
        public function down(): void
        {
            Schema::dropIfExists('users');
        }
    };
