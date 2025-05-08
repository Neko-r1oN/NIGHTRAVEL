@extends('layouts')
@section('title','ユーザーフォローリスト')
@section('body')

    <ul>
        <ul class="nav col-18 col-md-auto mb-2 justify-content-center mb-md-0">
            <li><a href="accounts" class="nav-link px-2 ">account</a></li>
            <li><a href="users" class="nav-link px-2  ">user</a></li>
            <li><a href="items" class="nav-link px-2 ">items</a></li>
            <li><a href="posItems" class="nav-link px-2">posItems</a></li>
            <li><a href="mails" class="nav-link px-2  link-">mails</a></li>
            <li><a href="posMails" class="nav-link px-2">posMails</a></li>
            <li><a href="followList" class="nav-link px-2 link-secondary">followList</a></li>
        </ul>
        <div class="form-group">
            <input
                type="text"
                class="form-control"
                id="search-box"
                placeholder="ユーザー名を入力(未対応です)"
            />
        </div>
        <button type="button" class="btn btn-success search-button">検索</button>
        <table class="table">
            <thead class="table-dark">
            <tr>
                <th>ID</th>
                <th>フォローしている</th>
                <th>フォローされている</th>
                <th>送信日時</th>
                <th></th>
            </tr>
            </thead>
            <tbody>
            @foreach($follows as $follow)
                <tr>
                    <td>{{$follow['id']}}</td>
                    <td>{{$follow['send_user_name']}}</td>
                    <td>{{$follow['follow_user_name']}}</td>
                    <td>{{$follow['created_at']}}</td>
                </tr>

        @endforeach
    </ul>

@endsection
