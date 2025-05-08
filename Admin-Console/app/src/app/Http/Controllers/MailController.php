<?php

    namespace App\Http\Controllers;


    use App\Http\Controllers\Controller;
    use App\Models\Login;
    use App\Models\Mail;
    use App\Models\mailLog;
    use App\Models\OpenMail;
    use App\Models\User;
    use Illuminate\Http\Request;


    class MailController extends Controller
    {
        public function index()
        {
            return view('send/index');
        }

        public function sent(Request $request)
        {
            //メールIDを取得
            $mailID = Mail::where('id', '=', $request['mail_id'])->get();
            //ユーザーIDを取得
            $userID = User::where('id', '=', $request['user_id'])->get();

            //存在しないメールIDが入力された場合
            if ($mailID->count() === 0) {
                return redirect()->route('send', ['error' => 'invalid']);
            }
            //存在しないユーザーIDが入力された場合
            if ($userID->count() === 0) {
                return redirect()->route('send', ['error' => 'invalid']);
            }

            //入力情報をDBに挿入
            OpenMail::create(['user_id' => $request['user_id'], 'mail_id' => $request['mail_id'], 'isOpen' => false]);

            //ログ生成
            MailLog::create([
                'open_user_id' => $request->user_id,
                'open_mail_id' => $request->mail_id,
                'action' => 1,
            ]);
            return redirect()->route('sent.index');
        }

        public function sentView(Request $request)
        {
            return view('sent/index');
        }
    }
