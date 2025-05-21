@extends('layouts.app')
@section('title','メール送信')
@section('send_mail','link-secondary')
@section('body')
    <h1>■メール送信■</h1>
    <p>メール送信が完了しました。</p>
    <a href="{{route('accounts.index')}}">アカウント一覧画面に戻る</a>
@endsection
