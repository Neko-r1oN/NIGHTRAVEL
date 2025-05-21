@extends('layouts.app')
@section('title','メールマスタデータ')
@section('mail','link-secondary')
@section('body')
    <h1>■メールマスタデータ■</h1>
    <table class="table">
        <tr>
            <th>ID</th>
            <th>タイトル</th>
            <th>内容</th>
            <th>配布アイテム</th>
            <th>個数</th>
        </tr>
        @foreach ($mails as $mail)
            <tr>
                <td>{{$mail['id']}}</td>
                <td>{{$mail['title']}}</td>
                <td>{{$mail['text']}}</td>
                <td>{{$mail['items_name']}}</td>
                <td>{{$mail['amount']}}</td>
            </tr>
        @endforeach
    </table>
@endsection
