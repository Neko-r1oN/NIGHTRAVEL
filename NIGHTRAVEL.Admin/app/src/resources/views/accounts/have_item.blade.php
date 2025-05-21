@extends('layouts.app')
@section('title','所持アイテム一覧')
@section('haveItems','link-secondary')
@section('body')
    <!--所持アイテム一覧表示-->
    <h1>■所持アイテム一覧■</h1>
    <form method="get" action={{ route('accounts.have-item') }}>
        <input type="text" name="id_find" placeholder="ユーザーIDを入力">
        <button type="submit">検索</button>
        @csrf
    </form>
    <table class="table">
        <tr>
            <th>ID</th>
            <th>プレイヤー名</th>
            <th>アイテム名</th>
            <th>所持個数</th>
        </tr>
        @if(!empty($user))
            @foreach ($user->items as $item)
                <tr>
                    <td>{{$item->id}}</td>
                    <td>{{$user->name}}</td>
                    <td>{{$item->name}}</td>
                    <td>{{$item->pivot->possession}}</td>
                </tr>
            @endforeach
        @endif
    </table>
@endsection
