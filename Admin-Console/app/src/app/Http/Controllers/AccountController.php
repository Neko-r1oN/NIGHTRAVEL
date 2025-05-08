<?php

    namespace App\Http\Controllers;

    use App\Models\Account;
    use App\Models\Achieve;
    use App\Models\Stage;
    use App\Models\User;
    use App\Models\Item;
    use App\Models\PosItem;
    use App\Models\Mail;
    use App\Models\OpenMail;
    use Illuminate\Http\Request;
    use Illuminate\Support\Facades\Hash;
    use Illuminate\Support\Facades\Validator;

    class AccountController extends Controller
    {
        public function index(Request $request)
        {

            //アカウント検索欄が空欄だった場合
            if (empty($request->account_name)) {
                //テーブルの全てのレコードを取得
                $accounts = Account::All();
                return view('accounts/index', ['accounts' => $accounts]);
            }//アカウント名の検索があった場合
            else {
                //指定文字列でフィルタリング
                $accounts = Account::where('name', '=', $request->account_name)->get();
                return view('accounts/index', ['accounts' => $accounts]);
            }

        }

        public function accountList(Request $request)
        {//
            //アカウント検索欄が空欄だった場合
            if (empty($request->account_name)) {
                //テーブルの全てのレコードを取得
                $accounts = Account::All();
                return view('accounts/index', ['accounts' => $accounts]);
            }//アカウント名の検索があった場合
            else {
                //指定文字列でフィルタリング
                $accounts = Account::where('name', '=', $request->account_name)->get();
                return view('accounts/index', ['accounts' => $accounts]);
            }
        }

        public function userList(Request $request)
        {//

            //テーブルの全てのレコードを取得
            $users = User::paginate(10);

            //$userI = User::find(1);


            return view('users.index', ['users' => $users]);

        }

        public function itemList(Request $request)
        {//

            //テーブルの全てのレコードを取得
            $items = Item::all();
            return view('items/index', ['stages' => $items]);


        }

        public function posItemList(Request $request)
        {

            //必要なレコードを取得
            $posItems = PosItem::select('pos_items.id', 'users.name as user_name', 'stages.name as item_name',
                'item_num')
                ->join('users', 'pos_items.user_id', '=', 'users.id')
                ->join('stages', 'pos_items.item_id', '=', 'stages.id')
                ->get();


            return view('posItems/index', ['posItems' => $posItems]);


        }

        public function mailList(Request $request)
        {//

            //必要なレコードを取得
            $mails = Mail::select('mails.id', 'mails.title as mail_title', 'mails.message as mail_message',
                'stages.name as item_name', 'mails.item_num')
                ->join('stages', 'mails.item_id', '=', 'stages.id')
                ->get();
            return view('mails/index', ['mails' => $mails]);


        }

        //メールリスト表示
        public function posMailList(Request $request)
        {//

            //必要なレコードを取得
            $posMails = OpenMail::select('open_mails.id', 'users.name as user_name', 'mails.title as mail_title',
                'mails.message as mail_message',
                'stages.name as item_name', 'mails.item_num', 'open_mails.isOpen')
                ->join('users', 'open_mails.user_id', '=', 'users.id')
                ->join('mails', 'open_mails.mail_id', '=', 'mails.id')
                ->join('stages', 'mails.item_id', '=', 'stages.id')
                ->get();


            return view('posMails/index', ['posMails' => $posMails]);
        }

        public function stageList(Request $request)
        {//

            //テーブルの全てのレコードを取得
            $stages = Stage::all();
            return view('stages/index', ['stages' => $stages]);


        }


        //アカウント登録画面表示
        public function showCreate(Request $request)
        {
            return view('create/index');
        }

        //アカウント登録完了画面表示
        public function created(Request $request)
        {
            return view('created.index', ['name' => $request['name'] ?? null]);
        }

        //アカウント更新画面表示
        public function showUpdate(Request $request)
        {
            $data = Account::where('id', '=', $request->id)->first();
            return view('update.index', ['account' => $data]);
        }

        public function update(Request $request)
        {
            $validator = Validator::make($request->all(), [
                'password' => ['required']
            ]);

            if ($validator->fails()) {
                return redirect()->route('accounts.showUpdate')
                    ->withErrors($validator)
                    ->withInput();
            }


            $account = Account::findOrFail($request->id);
            $account->password = Hash::make($request->password);
            $account->save();

            return redirect()->route('index', ['name' => $account->name]);

        }


        public function create(Request $request)
        {
            return view('create/index', ['name' => $request['name'] ?? null]);
        }

        //アカウント登録処理
        public function store(Request $request)
        {

            $validator = Validator::make($request->all(), [
                'name' => ['required', 'max:255'],
                'password' => ['required']
            ]);

            if ($validator->fails()) {
                return redirect()->route('accounts.create')
                    ->withErrors($validator)
                    ->withInput();
            }

            //再入力パスワードと異なる場合
            if ($request['password'] !== $request['password2']) {
                return redirect()->route('accounts.create', ['error' => 'noMatch']);
            }

            //テーブルから入力されたnameを取得
            $account = Account::where('name', '=', $request['name'])->get();

            //nameが被っていなかった場合
            if ($account->count() == 0) {
                //入力情報をDBに挿入
                Account::create(['name' => $request['name'], 'password' => Hash::make($request['password'])]);
            } else {

                return redirect()->route('accounts.create', ['error' => 'invalid']);
            }


            return redirect()->route('accounts.created');
        }

        public function destroy(Request $request)
        {
            $account = Account::findOrFail($request->id);
            $account->delete();

            return redirect()->route('index', ['deleted' => $account->name]);
        }
    }
