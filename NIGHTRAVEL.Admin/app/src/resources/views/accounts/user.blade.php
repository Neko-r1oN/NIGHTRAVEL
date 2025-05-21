@extends('layouts.app')
@section('title','ユーザー一覧')
@section('users','link-secondary')
@section('body')
    <!--プレイヤー一覧表示-->
    <h1>■ユーザー一覧■</h1>
    {{$users->onEachSide(1)->links('vendor.pagination.bootstrap-5')}}
    <table class="table">
        <tr>
            <th>ID</th>
            <th>名前</th>
            <th>レベル</th>
            <th>経験値</th>
            <th>ライフ</th>
        </tr>
        @foreach ($users as $user)
            <tr>
                <td>{{$user['id']}}</td>
                <td>{{$user['name']}}</td>
                <td>{{$user['level']}}</td>
                <td>{{$user['experience_point']}}</td>
                <td>{{$user['life']}}</td>
            </tr>
        @endforeach
    </table>
@endsection
