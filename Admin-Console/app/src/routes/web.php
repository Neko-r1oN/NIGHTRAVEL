<?php

    use App\Http\Controllers\AccountController;
    use App\Http\Controllers\AuthController;
    use App\Http\Controllers\LogController;
    use App\Http\Controllers\MailController;
    use App\Http\Controllers\UserController;
    use App\Http\Middleware\AuthMiddleware;
    use App\Http\Middleware\NocheMiddleware;
    use Illuminate\Support\Facades\Route;

    //キャッシュ無効(デフォルト)
    Route::middleware([NocheMiddleware::class])->group(function () {

        //ログイン画面表示(デフォルト)
        //Route::get('/', [AuthController::class, 'showLogin'])->name('login.index');
        //Route::get('/login', [AuthController::class, 'showLogin'])->name('login');

        //ユーザーアカウントリスト表示
        Route::get('index', [AccountController::class, 'index'])->name('index');

        //ログイン画面
        Route::get('/', [AuthController::class, 'index'])->name('login.index');
        //ログイン処理
        Route::post('auth/doLogin', [AuthController::class, 'doLogin'])->name('login');
        //ログアウト処理
        Route::post('auth/doLogout', [AuthController::class, 'doLogout'])->name('logout');


        Route::middleware(AuthMiddleware::class)->group(function () {
            Route::get('accounts', [AccountController::class, 'accountList'])->name('accounts.index');

            //ユーザーリスト表示
            Route::get('users', [AccountController::class, 'userList'])->name('users.index');
            //ステージリスト表示
            Route::get('stages', [AccountController::class, 'stageList'])->name('stages.index');
            //アイテムリスト表示
            Route::get('items', [AccountController::class, 'itemList'])->name('stages.index');
            //所持アイテムリスト表示
            Route::get('posItems', [AccountController::class, 'posItemList'])->name('posItems.index');
            //マスターメールリスト表示
            Route::get('mails', [AccountController::class, 'mailList'])->name('mails.index');
            //メールリスト表示
            Route::get('posMails', [AccountController::class, 'posMailList'])->name('posMails.index');
            //ユーザーフォローリスト表示
            Route::get('followList', [UserController::class, 'followList'])->name('follows.index');

            Route::get('achieveList', [AccountController::class, 'stageList'])->name('stages.index');

            //スコアログ表示
            Route::get('scoreLogs', [LogController::class, 'scoreLogs'])->name('scoreLogs.index');
            /*
            //アイテムログ表示
            Route::get('scoreLogs', [LogController::class, 'itemLogs'])->name('scoreLogs.index');
            //アイテムログ表示
            Route::get('followLogs', [LogController::class, 'followLogs'])->name('followLogs.index');
            //アイテムログ表示
            Route::get('mailLogs', [LogController::class, 'mailLogs'])->name('mailLogs.index');
*/

            #メール送信画面表示
            Route::get('send', [MailController::class, 'index'])->name('send.index');

            Route::post('sent', [MailController::class, 'sent'])->name('sent');

            Route::get('sentView', [MailController::class, 'sentView'])->name('sent.index');
        });

        Route::prefix('accounts')->name('accounts.')->controller(AccountController::class)
            ->middleware(AuthMiddleware::class)->group(function () {

                //ユーザーアカウントリスト表示
                Route::get('index', [AccountController::class, 'index'])->name('index');
                //アカウント登録画面
                Route::get('create', [AccountController::class, 'create'])->name('create');
                //アカウント登録完了画面
                Route::get('created', [AccountController::class, 'created'])->name('created');
                //アカウント更新画面
                Route::get('showUpdate', 'showUpdate')->name('showUpdate');


                //アカウント編集処理
                Route::post('{id}/edit', 'edit')->name('edit');
                //アカウント登録処理
                Route::post('store', 'store')->name('store');
                //アカウント更新処理
                Route::post('{id}/update', 'update')->name('update');
                //アカウント削除処理
                Route::get('{id}/destroy', 'destroy')->name('destroy');
            });
        //Route::resource('accounts', 'AccountController')->only(['index', 'store', 'update', 'destroy']);


    });

