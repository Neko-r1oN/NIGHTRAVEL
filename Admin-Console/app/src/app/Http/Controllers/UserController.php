<?php

    namespace App\Http\Controllers;

    use App\Models\Follow;
    use Illuminate\Http\Request;

    class UserController extends Controller
    {
        public function index()
        {

        }

        //ユーザーフォローリスト表示
        public function followList(Request $request)
        {//

            //必要なレコードを取得
            $follows = Follow::select('follows.id', 'follows.send_user_id as send_user_name',
                'follows.follow_user_id as follow_user_name',
                'follows.created_at')
                ->join('users', 'follows.send_user_id', '=', 'users.id')
                ->get();
            return view('follows/index', ['follows' => $follows]);


        }
    }
