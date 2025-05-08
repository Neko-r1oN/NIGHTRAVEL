@extends('layouts')
@section('title','管理者アカウント一覧')
@section('body')

    <ul>
        <ul class="nav col-18 col-md-auto mb-2 justify-content-center mb-md-0">
            <li><a href="accounts" class="nav-link px-2 link-secondary">account</a></li>
            <li><a href="stages" class="nav-link">stages</a></li>
        </ul>
       
        <table class="table">
            <thead class="table-dark">
            <tr>
                <th>ID</th>
                <th>名前</th>
                <th>パスワード</th>
                <th>アカウント操作</th>
                <th></th>
            </tr>
            </thead>
            <tbody>
            @foreach($accounts as $account)
                <tr>
                    <td>{{$account['id']}}</td>
                    <td>{{$account['name']}}</td>
                    <td>{{$account['password']}}</td>
                    <td>
                        <a href="{{ route('accounts.destroy', ['id'=>$account['id']]) }}"
                           class="btn btn-danger">削除</a>
                        <a href="{{ route('accounts.showUpdate', ['id'=>$account['id']]) }}"
                           class="btn btn-success">更新</a>
                    </td>
                </tr>

        @endforeach
    </ul>
    <div class="col-md-3 text-end">


        <form method="GET" action="{{url('accounts/create')}}">
            @csrf
            <button type="submit" class="btn btn-outline-primary me-2">アカウント登録</button>
        </form>
    </div>
@endsection
