<?php

namespace App\Http\Controllers;


use App\Http\Resources\FollowResource;
use App\Models\Account;
use App\Models\FollowLists;
use App\Models\HaveItem;
use App\Models\Item;
use App\Models\Mail;
use App\Models\User;
use App\Models\UserMail;
use Barryvdh\Debugbar\Facades\Debugbar;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Hash;
use function Laravel\Prompts\password;

class AccountController extends Controller
{
    //アカウント一覧を表示する
    public function index(Request $request)
    {
        if (isset($request['name_index'])) {
            //nameが指定されていたら
            $accounts = Account::where('name', '=', $request['name_index'])->get();
        } else {
            //テーブルの全てのレコードを表示
            $accounts = Account::All();
        }

        //デバッグ

        //AccountControllerのindex関数に指定したIDを渡せる。※dd関数はデバッグ用表示
        //dd($request->account_id);

        //DebugBar::info('てりやきマックうまかった');
        //DebugBar::error('チキチー食べたい');

        //セッションに指定のキーで値を保存
        //$request->session()->put('key', 5);
        //$request->session()->put('key2', 8);

        //セッションから指定のキーの値を取得
        //$value = $request->session()->get('key');

        //DebugBar::info($value);

        //指定したデータをセッションから削除
        //$request->session()->forget('key');
        //$value = $request->session()->get('key');
        //DebugBar::info($value);

        //セッションの全てのデータを削除
        //$request->session()->flush();
        //$value = $request->session()->get('key2');
        //DebugBar::info($value);

        //セッションに指定したキーが存在するか
        if ($request->session()->exists('login')) {
            return view('accounts/index', ['accounts' => $accounts]);             //ビューに変数を渡す
        } else {
            return view('accounts/login');
        }
    }

    //ログイン画面表示
    public function login(Request $request)
    {
        return view('accounts/login');
    }

    //ログイン処理
    public function doLogin(Request $request)
    {
        //バリデーション
        $validated = $request->validate([
            'name' => ['required', 'min:4', 'max:20']
        ]);

        //条件を指定して取得
        $account = Account::where('name', '=', $request['name'])->get();

        if (Hash::check($request['pass'], $account[0]->password)) {
            //成功した時
            //セッションに指定のキーで値を保存
            $request->session()->put('login', true);
            return redirect()->route('accounts.index');
        } else {
            //失敗した時
            return redirect()->route('login', ['error' => 'invalid']);
        }
    }

    public function doLogout(Request $request)
    {
        //指定したデータをセッションから削除
        $request->session()->forget('login');

        return redirect()->route('login');
    }

    //プレイヤーリスト
    public function userList(Request $request)
    {
        $users = User::All();
        //ページャー
        $users = User::paginate(10);

        //セッションに指定したキーが存在するか
        if ($request->session()->exists('login')) {
            return view('accounts/user', ['users' => $users]);             //ビューに変数を渡す
        } else {
            return view('accounts/login');
        }
    }

    //アイテムリスト
    public function itemList(Request $request)
    {
        $items = Item::All();

        //セッションに指定したキーが存在するか
        if ($request->session()->exists('login')) {
            return view('accounts/item', ['items' => $items]);             //ビューに変数を渡す
        } else {
            return view('accounts/login');
        }
    }

    //プレイヤー所持アイテムリスト
    public function have_ItemList(Request $request)
    {
        $user = User::find($request['id_find']);
        //関連モデルからデータ取得
        return view('accounts.have_item', ['user' => $user ?? null]);
    }

    //アカウント追加画面
    public function addAccount(Request $request)
    {
        return view('accounts/add');
    }

    //アカウント追加処理
    public function doAdd(Request $request)
    {
        //バリデーション
        $validated = $request->validate([
            'name' => ['required', 'unique:accounts,name', 'min:4', 'max:20'],
            'pass' => ['required', 'confirmed', 'unique:accounts,password']
        ]);

        $pass = Hash::make($request['pass']);

        //レコードを追加
        Account::create(['name' => $request['name'], 'password' => $pass]);

        return redirect()->route('accounts.complete');
    }

    //アカウント追加完了画面
    public function addComplete(Request $request)
    {
        return view('accounts/complete');
    }

    //アカウント削除画面
    public function delete(Request $request)
    {
        return view('accounts.delete', ['id' => $request['id']]);
    }

    //アカウント削除処理
    public function doDelete(Request $request)
    {
        //idで検索後にレコードを削除
        $account = Account::findOrFail($request['id']);
        $account->delete();

        return redirect()->route('accounts.completeDel');
    }

    //アカウント削除完了画面
    public function completeDel(Request $request)
    {
        return view('accounts.completeDel');
    }

    //アカウント更新画面
    public function update(Request $request)
    {
        return view('accounts.update', ['id' => $request['id']]);
    }

    //アカウント更新処理
    public function doUpdate(Request $request)
    {
        //idで検索後にレコードを更新
        $account = Account::findOrFail($request['id']);

        //取得してきたパスワードに入力したパスワード（ハッシュ化）を代入して更新する
        $account->password = Hash::make($request['password']);

        //保存
        $account->save();

        //完了画面にリダイレクト
        return redirect()->route('accounts.completeUpdate');
    }

    //アカウント更新完了画面
    public function completeUpdate(Request $request)
    {
        return view('accounts.completeUpdate');
    }

    //メールマスタデータ表示
    public function mailList(Request $request)
    {
        $mails = Mail::select([
            'mails.id as id',
            'mails.title as title',
            'mails.text as text',
            'items.name as items_name',
            'amount'
        ])
            ->join('items', 'items.id', '=', 'mails.item_id')
            ->get();

        return view('accounts.mail', ['mails' => $mails]);
    }

    //ユーザー受信メール一覧
    public function userMail(Request $request)
    {
        $user_mails = UserMail::select([
            'user_mails.id as id',
            'users.name as user_name',
            'mails.text as mail_name',
            'user_mails.open_flag as open_flag'
        ])
            ->join('users', 'users.id', '=', 'user_mails.user_id')
            ->join('mails', 'mails.id', '=', 'user_mails.mail_id')
            ->get();

        return view('accounts.user_mail', ['user_mails' => $user_mails]);
    }

    //メール送信画面
    public function sendMail(Request $request)
    {
        $mails = Mail::all();

        return view('accounts.send_mail', ['mails' => $mails]);
    }

    //メール送信処理
    public function doSend(Request $request)
    {
        //バリデーションチェック
        //user_idが存在するか
        $users = User::all();
        foreach ($users as $user) {
            //配列分回す
            if ($request['user_id'] == $user['id']) {
                //持ってきたidとDBのデータのidが一致するか確かめる
                //レコードを追加
                UserMail::create([
                    'user_id' => $request['user_id'],
                    'mail_id' => $request['mail_id'],
                    'open_flag' => false
                ]);
                //完了画面にリダイレクト
            }
        }
        //一致しなかったら
        //入力画面にリダイレクト
        return redirect()->route('accounts.sendmail');
    }

    //送信完了画面
    public function completeSend(Request $request)
    {
        return view();
    }

    //フォローリスト
    public function followList()
    {
        $follows = FollowLists::all();
        return view('accounts.follow_list', ['follows' => $follows]);
    }
}
