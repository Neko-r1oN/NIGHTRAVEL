<?php

namespace App\Http\Controllers;

use App\Http\Resources\MailResource;
use App\Models\HaveItem;
use App\Models\Mail;
use App\Models\UserMail;
use http\Env\Response;
use Illuminate\Http\Request;
use Mockery\Exception;
use Illuminate\Support\Facades\DB;

class MailController extends Controller
{
    public function index()
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

        return response()->json(
            MailResource::collection($mails)
        );
    }

    //メール受け取り処理　※ゲーム内未使用
    public function openMail(Request $request)
    {
        try {
            //トランザクション処理
            DB::transaction(function () use ($request) {
                //UserMailからuser_idとmail_idが一致するデータを取得
                $userMails = UserMail::where('user_id', '=', $request->user_id)
                    ->where('mail_id', '=', $request->mail_id)
                    ->get();

                //データがなかったらエラー
                if (count($userMails) === 0) {
                    return response()->json([], 400);
                }

                //カラム更新
                if ($userMails->first()->open_flag === 1) {
                    //もう開封済みだったらエラー
                    return response()->json([], 400);
                }
                $userMails->first()->open_flag = 1;

                //保存
                $userMails->first()->save();

                $mails = Mail::findOrFail($request->mail_id);

                $userItems = HaveItem::where('user_id', '=', $request->user_id)
                    ->where('item_id', '=', $mails->item_id)
                    ->get();

                if (count($userItems) === 0) {
                    $items = HaveItem::create([
                        'user_id' => $request->user_id,
                        'item_id' => $mails->item_id,
                        'possession' => $mails->amount
                    ]);
                } else {
                    $userItems->first()->possession += $mails->amount;

                    //保存
                    $userItems->first()->save();
                }
                return response()->json();
            });
        } catch (Exception $e) {
            return response()->json($e, 500);
        }
    }
}
