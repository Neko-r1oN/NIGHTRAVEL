@extends('layouts.app')
@section('title','ユーザー受信メール一覧')
@section('user_mail','link-secondary')
@section('body')
    <h1>■ユーザー受信メール一覧■</h1>
    <table class="table">
        <tr>
            <th>ID</th>
            <th>ユーザー名</th>
            <th>メール名</th>
            <th>開封/未開封</th>
        </tr>
        @foreach ($user_mails as $user_mail)
            <tr>
                <td>{{$user_mail['id']}}</td>
                <td>{{$user_mail['user_name']}}</td>
                <td>{{$user_mail['mail_name']}}</td>
                @if($user_mail['open_flag'])
                    <td>開封済み</td>
                @else
                    <td>未開封</td>
                @endif
                <td></td>
            </tr>
        @endforeach
    </table>
@endsection
