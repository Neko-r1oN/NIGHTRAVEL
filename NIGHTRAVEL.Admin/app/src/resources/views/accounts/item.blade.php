@extends('layouts.app')
@section('title','アイテム一覧')
@section('items','link-secondary')
@section('body')
    <!--アイテム一覧表示-->
    <h1>■アイテム一覧■</h1>
    <table class="table">
        <tr>
            <th>ID</th>
            <th>名前</th>
            <th>種別</th>
            <th>効果値</th>
            <th>説明</th>
        </tr>
        @foreach ($items as $item)
            <tr>
                <td>{{$item['id']}}</td>
                <td>{{$item['name']}}</td>
                <td>{{$item['type']}}</td>
                <td>{{$item['effect']}}</td>
                <td>{{$item['info']}}</td>
            </tr>
        @endforeach
    </table>
@endsection
