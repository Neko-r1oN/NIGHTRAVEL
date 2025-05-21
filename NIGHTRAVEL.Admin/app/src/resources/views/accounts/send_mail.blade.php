@extends('layouts.app')
@section('title','メール送信')
@section('send_mail','link-secondary')
@section('body')
    <h1>■メール送信■</h1>
    <form method="post" action="{{ route('accounts.doSend') }}">
        <input type="text" name="user_id" placeholder="ユーザーIDを入力" required>
        <select class="form-select" required aria-label="select example" name="mail_id" style="width: 30%;">
            @foreach ($mails as $mail)
                <option value="{{$mail['id']}}">{{$mail['title']}}</option>
            @endforeach
        </select>
        <button type="submit">送信</button>
        @csrf
    </form>
@endsection
