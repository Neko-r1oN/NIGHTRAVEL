@extends('layouts')
@section('title','ユーザー一覧')
@section('body')

    <ul>

        <ul class="nav col-18 col-md-auto mb-2 justify-content-center mb-md-0">
            <li><a href="users" class="nav-link px-2 link-secondary">users</a></li>
            <li><a href="scoreLogs" class="nav-link">log</a></li>
        </ul>
        <div class="justify-content-center">
            {{$users->links('vendor.pagination.bootstrap-5')}}
        </div>
        <table class="table">
            <thead class="table-dark">
            <tr>
                <th>No</th>
                <th>名前</th>
                <th>パスワード(ハッシュ表示)</th>


            </tr>
            </thead>
            <tbody>
            @foreach($users as $user)

                <tr>
                    <th>{{$user['id']}}</th>
                    <th>{{$user['name']}}</th>
                    <th>{{$user['password']}}</th>

                </tr>

            @endforeach
            </tbody>
        </table>
    </ul>

@endsection
