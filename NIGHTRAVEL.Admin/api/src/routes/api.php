<?php

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;

//ユーザー指定表示
Route::middleware(\App\Http\Middleware\NoCacheMiddleware::class)
    ->get('users/{user_id}', [\App\Http\Controllers\UserController::class, 'show'])
    ->name('users.show');
//ユーザー一覧
Route::middleware(\App\Http\Middleware\NoCacheMiddleware::class)
    ->get('users', [\App\Http\Controllers\UserController::class, 'index'])
    ->name('users.index');
//アイテム一覧
Route::middleware(\App\Http\Middleware\NoCacheMiddleware::class)
    ->get('items', [\App\Http\Controllers\ItemController::class, 'index'])
    ->name('items.index');
//メール一覧
Route::middleware(\App\Http\Middleware\NoCacheMiddleware::class)
    ->get('mails', [\App\Http\Controllers\MailController::class, 'index'])
    ->name('mails.index');
//ユーザー所持アイテム一覧
Route::middleware(\App\Http\Middleware\NoCacheMiddleware::class)
    ->get('usersItem/{user_id}', [\App\Http\Controllers\UserController::class, 'userItem'])
    ->name('users.userItem');
//ユーザー受信メール一覧
Route::middleware(\App\Http\Middleware\NoCacheMiddleware::class)
    ->get('mails/{user_id}', [\App\Http\Controllers\UserController::class, 'userMail'])
    ->name('mail.userMail');
//フォローリスト取得
Route::middleware(\App\Http\Middleware\NoCacheMiddleware::class)
    ->get('follows{user_id}', [\App\Http\Controllers\UserController::class], 'userFollow')
    ->name('users.userFollow');
//ユーザー登録
Route::post('users/store', [\App\Http\Controllers\UserController::class, 'store'])
    ->name('users.store');
//ユーザー更新
Route::post('users/update', [\App\Http\Controllers\UserController::class, 'update'])
    ->middleware('auth:sanctum')->name('users.update');
//メール受け取り
Route::post('mails/open', [\App\Http\Controllers\MailController::class, 'openMail'])
    ->middleware('auth:sanctum')->name('mails.open');
//クリエイトステージ一覧取得
Route::middleware(\App\Http\Middleware\NoCacheMiddleware::class)
    ->get('creates/index', [\App\Http\Controllers\CreateStageController::class, 'index'])
    ->name('creates.index');
//ステージクリエイト情報登録
Route::post('creates/store', [\App\Http\Controllers\CreateStageController::class, 'store'])
    ->name('creates.store');
//ステージクリエイト情報取得
Route::middleware(\App\Http\Middleware\NoCacheMiddleware::class)
    ->get('creates/show/{create_stage_id}', [\App\Http\Controllers\CreateStageController::class, 'show'])
    ->name('creates.show');
