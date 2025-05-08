@extends('layouts')
@section('title','メールログ')
@section('body')

    <ul>
        <ul class="nav col-18 col-md-auto mb-2 justify-content-center mb-md-0">
            <li><a href="itemLogs" class="nav-link px-2  ">itemLog</a></li>
            <li><a href="followLogs" class="nav-link px-2">followLog</a></li>
            <li><a href="mailLogs" class="nav-link px-2 link-secondary">mailLog</a></li>
        </ul>
        <table class="table">
            <thead class="table-dark">
            <tr>
                <th>ログID</th>
                <th>送信先ユーザーID</th>
                <th>メールID</th>
                <th>アクション</th>
                <th>生成日時</th>
            </tr>
            </thead>
            <tbody>
            @foreach($logs as $log)
                <tr>
                    <td>{{$log['id']}}</td>
                    <td>{{$log['open_user_id']}}</td>
                    <td>{{$log['open_user_id']}}</td>
                    <td>
                        @if($log['action'] === 1)
                            送信
                        @else
                            開封
                        @endif
                    </td>
                    <td>{{$log['created_at']}}</td>

                </tr>

        @endforeach
    </ul>

@endsection

