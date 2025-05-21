<?php

use App\Http\Controllers\AccountController;
use App\Http\Middleware\AuthMiddleware;
use Illuminate\Support\Facades\Route;

//ルートのグループ化
Route::prefix('accounts')->name('accounts.')->controller(AccountController::class)
    ->middleware(AuthMiddleware::class)
    ->middleware(\App\Http\Middleware\NoCacheMiddleware::class)
    ->group(function () {
        Route::get('user', 'userList')->name('user-list');                      //userのルート
        Route::get('add', 'addAccount')->name('create');                        //アカウント登録のルーティング追加
        Route::post('doAdd', 'doAdd')->name('store');                           //アカウント登録処理のルーティング追加
        Route::get('complete', 'addComplete')->name('complete');                //アカウント追加完了画面のルーティング追加
        Route::post('delete', 'delete')->name('delete');                        //アカウント削除画面のルート
        Route::post('doDelete', 'doDelete')->name('doDelete');                  //アカウント削除処理のルート
        Route::get('completeDel', 'completeDel')->name('completeDel');         //アカウント削除完了のルート
        Route::post('update', 'update')->name('update');                         //アカウントの更新画面のルート
        Route::post('doUpdate', 'doUpdate')->name('doUpdate');                         //アカウントの更新画面のルート
        Route::get('completeUpdate', 'completeUpdate')->name('completeUpdate');       //アカウントの更新完了のルート
        Route::get('mail', 'mailList')->name('mailList');                            //メールのマスタデータのルート
        Route::get('user_mail', 'usermail')->name('user_mail');                    //ユーザー受信メール一覧のルート
        Route::get('send_mail', 'sendmail')->name('sendmail');                       //メール送信画面表示のルート
        Route::post('doSend', 'doSend')->name('doSend');                             //メール送信処理
        Route::get('completeSend', 'completeSend')->name('completeSend');             //メール送信完了画面のルート
        Route::get('follow', 'followList')->name('followList');
    });

Route::get('/', [AccountController::class, 'login'])->name('login');

//doLoginのルーティング追加
Route::post('accounts/doLogin', [AccountController::class, 'doLogin'])->name('doLogin');

//doLogoutのルーティング追加
Route::post('accounts/doLogout', [AccountController::class, 'doLogout'])->name('doLogout');

//itemのルーティング追加
Route::get('accounts/item', [AccountController::class, 'itemList'])->name('accounts.item');

//have_itemのルーティング追加
Route::get('accounts/have_item', [AccountController::class, 'have_ItemList'])->name('accounts.have-item');

Route::get('accounts/index/{account_id?}', [AccountController::class, 'index'])->name('accounts.index');

Route::post('accounts/index', [AccountController::class, 'index']);
