@extends('layouts.app')
@section('title','フォロー一覧')
@section('follow','link-secondary')
@section('body')
    <!--アイテム一覧表示-->
    <h1>■フォロー一覧■</h1>
    <table class="table">
        <tr>
            <th>ID</th>
            <th>ユーザー名</th>
            <th>フォローしたユーザー名</th>
        </tr>
        @foreach ($follows as $follow)
            <tr>
                <td>{{$follow['id']}}</td>
                <td>{{$follow->user->name}}</td>
                <td>{{$follow->followUser->name}}</td>
            </tr>
        @endforeach
    </table>
@endsection
