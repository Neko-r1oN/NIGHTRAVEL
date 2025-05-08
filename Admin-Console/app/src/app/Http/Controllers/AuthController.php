<?php

    namespace App\Http\Controllers;

    use App\Models\Account;
    use Barryvdh\Debugbar\Facades\Debugbar;
    use Illuminate\Support\Facades\Validator;
    use Illuminate\Http\Request;
    use Illuminate\Support\Facades\Hash;


    class AuthController extends Controller
    {
        //ログイン画面を表示
        public function index(Request $request)
        {

            return view('login.index', ['error' => $request['error'] ?? null]);

        }

        //ログイン処理
        public function doLogin(Request $request)
        {
            route('login');

            //バリデーションチェック
            $validator = Validator::make($request->all(), [
                'name' => ['required', 'min:4'],
                'password' => 'required',
            ]);

            if ($validator->fails()) {
                return redirect("/")
                    ->withErrors($validator)
                    ->withInput();
            }

            //レコードを取得
            $account = Account::where('name', '=', $request['name'])->get();


            //レコードを取得・ハッシュ化したキーと一致した場合
            if ($account->count() > 0 && Hash::check($request['password'], $account[0]->password)) {

                //ログイン情報を保存
                $request->session()->put('login', true);
                return redirect()->route('index');

            }
            //キーと不一致、存在しないユーザー名だった場合
            //return redirect()->route('login', ['error' => 'invalid']);
            // エラー表示
            return redirect('/');
            //return redirect()->route('login.index', ['error' => 'invalid']);
        }

        //ログアウト処理
        public function doLogout(Request $request)
        {
            //指定したデータをセッションから削除
            $request->session()->forget('login');

            //ログイン画面にリダイレクト
            return redirect('/');
        }


    }
