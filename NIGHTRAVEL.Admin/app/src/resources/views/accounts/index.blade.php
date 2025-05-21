@extends('layouts.app')
@section('title','アカウント一覧')
@section('accounts','link-secondary')
@section('body')
    <h1>■アカウント一覧</h1>
    <form method="post" action="/accounts/index">
        <input type="text" name="name_index" placeholder="名前を入力">
        <button type="submit">検索</button>
        @csrf
    </form>

    <table class="table">
        <tr>
            <th>ID</th>
            <th>名前</th>
            <th>パス</th>
            <th>操作</th>
        </tr>
        @foreach($accounts as $account)
            <tr>
                <td>{{$account['id']}}</td>
                <td>{{$account['name']}}</td>
                <td>{{$account['password']}}</td>
                <td>
                    <form method="post" action="{{route('accounts.delete')}}">
                        @csrf
                        <button type='submit'>削除</button>
                        <input type="hidden" name="id" value="{{$account['id']}}">
                    </form>
                    <form method="post" action="{{route('accounts.update')}}">
                        @csrf
                        <button type='submit'>更新</button>
                        <input type="hidden" name="id" value="{{$account['id']}}">
                    </form>
                </td>
            </tr>
        @endforeach
    </table>
@endsection
