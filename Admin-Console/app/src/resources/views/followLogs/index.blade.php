@extends('layouts')
@section('title','フォローログ')
@section('body')

    <ul>
        <ul class="nav col-18 col-md-auto mb-2 justify-content-center mb-md-0">
            <li><a href="itemLogs" class="nav-link px-2  ">itemLog</a></li>
            <li><a href="followLogs" class="nav-link px-2 link-secondary">followLog</a></li>
            <li><a href="mailLogs" class="nav-link px-2 ">mailLog</a></li>
        </ul>
        <table class="table">
            <thead class="table-dark">
            <tr>
                <th>ログID</th>
                <th>フォローしたユーザー</th>
                <th>フォローされたユーザー</th>
                <th>アクション</th>
                <th>生成日時</th>
            </tr>
            </thead>
            <tbody>
            @foreach($logs as $log)
                <tr>
                    <td>{{$log['id']}}</td>
                    <td>{{$log['follow_user_id']}}</td>
                    <td>{{$log['follower_user_id']}}</td>
                    <td>
                        @if($log['action'] === 1)
                            フォロー
                        @else
                            フォロー解除
                        @endif
                    </td>
                    <td>{{$log['created_at']}}</td>

                </tr>

        @endforeach
    </ul>

@endsection

