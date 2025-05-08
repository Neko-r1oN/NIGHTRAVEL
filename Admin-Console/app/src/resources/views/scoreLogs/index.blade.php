@extends('layouts')
@section('title','スコアログ')
@section('body')

    <ul>
        <ul class="nav col-18 col-md-auto mb-2 justify-content-center mb-md-0">
            <li><a href="users" class="nav-link ">users</a></li>
            <li><a href="scoreLogs" class="nav-link px-2 link-secondary">log</a></li>
        </ul>

       
        <table class="table">
            <thead class="table-dark">
            <tr>
                <th>ログID</th>
                <th>ユーザーID</th>
                <th>ユーザー名</th>
                <th>スコア</th>
                <th>プレイ日時</th>
            </tr>
            </thead>
            <tbody>
            @foreach($logs as $log)
                <tr>
                    <td>{{$log['id']}}</td>
                    <td>{{$log['user_id']}}</td>
                    <td>{{$log['user_name']}}</td>
                    <td>{{$log['score']}}</td>
                    <td>{{$log['created_at']}}</td>

                </tr>

        @endforeach
    </ul>

@endsection

