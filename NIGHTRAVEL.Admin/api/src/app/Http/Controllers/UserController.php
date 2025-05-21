<?php

namespace App\Http\Controllers;

use App\Http\Resources\UserItemResource;
use App\Http\Resources\UserMailResource;
use App\Http\Resources\UserResource;
use App\Models\FollowLists;
use App\Models\User;
use Illuminate\Support\Facades\Validator;
use http\Env\Response;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\DB;
use Mockery\Exception;

class UserController extends Controller
{
    public function show(Request $request)
    {
        $user = User::findOrFail($request->user_id);
        $response = [
            'detail' => $user
        ];
        return response()->json(
            UserResource::make($user)              //UserResource::makeでuserを変換してからJsonを作成
        );
    }

    //ユーザーリストを取得
    public function index(Request $request)
    {
        //バリデーションチェック
        $validator = Validator::make($request->all(), [
            'min_level' => ['required', 'int'],
            'max_level' => ['required', 'int'],
        ]);
        if ($validator->fails()) {
            return response()->json($validator->errors(), 400);
        }

        $users = User::where('level', '>', $request->min_level)
            ->where('level', '<', $request->max_level);

        return response()->json(
            UserResource::collection($users)
        );
    }

    //ユーザー所持アイテムリスト　※ゲーム内未使用
    public function userItem(Request $request)
    {
        //指定ユーザーを取得
        $user = User::findOrFail($request->user_id);

        //所持アイテムリストをリレーションで取得
        $items = $user->items;

        //中間テーブルのリソースを使いJSON化
        $response['items'] = UserItemResource::collection($items);

        return response()->json($response);
    }

    //ユーザー受信メールリスト　※ゲーム内未使用
    public function userMail(Request $request)
    {
        //指定ユーザーを取得
        $user = User::findOrFail($request->user_id);

        //所持アイテムリストをリレーションで取得
        $mails = $user->mails;

        //中間テーブルのリソースを使いJSON化
        $response['mails'] = UserMailResource::collection($mails);

        return response()->json($response);
    }

    //ユーザー登録
    public function store(Request $request)
    {
        //バリデーションチェック
        $validator = Validator::make($request->all(), [
            'name' => ['required', 'string']
        ]);
        if ($validator->fails()) {
            return response()->json($validator->errors(), 400);
        }

        try {
            //トランザクション処理
            $user = DB::transaction(function () use ($request) {
                $user = User::create([
                    'name' => $request->name,
                    'level' => 1,
                    'experience_point' => 1,
                    'life' => 1
                ]);
                return $user;
            });
            //APIトークンを発行する
            $token = $user->createToken($request->name)->plainTextToken;
            return response()->json(['id' => $user->id, 'token' => $token]);
        } catch (Exception $e) {
            return response()->json($e, 500);
        }


    }

    //ユーザー更新
    public function update(Request $request)
    {
        try {
            //トライザクション処理
            DB::transaction(function () use ($request) {
                $user = User::findOrFail($request->user_id);

                //カラムの更新
                if (isset($request->level)) {
                    $user->level = $request->level;
                }

                //保存
                $user->save();
            });
            return response()->json();
        } catch (Exception $error) {
            return response()->json($error, 500);
        }
    }

    //フォローリスト取得　※ゲーム内未使用
    public function userFollow(Request $request)
    {
        $user = FollowLists::findOrFail($request->user_id);

        //ユーザーリストをリレーションで取得
        $follows = $user->follow;

        $response['follows'] = UserMailResource::collection($follows);

        return response()->json($response);
    }
}
