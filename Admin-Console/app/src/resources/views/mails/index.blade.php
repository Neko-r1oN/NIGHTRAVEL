@extends('layouts')
@section('title','メールマスターデータ')
@section('body')

    <ul>
        <ul class="nav col-18 col-md-auto mb-2 justify-content-center mb-md-0">
            <li><a href="accounts" class="nav-link px-2">account</a></li>
            <li><a href="users" class="nav-link px-2 ">user</a></li>
            <li><a href="items" class="nav-link px-2 ">items</a></li>
            <li><a href="posItems" class="nav-link px-2 ">posItems</a></li>
            <li><a href="mails" class="nav-link px-2  link-secondary">mails</a></li>
            <li><a href="posMails" class="nav-link px-2">posMails</a></li>
            <li><a href="followList" class="nav-link px-2 ">followList</a></li>
        </ul>
        <table class="table">
            <thead class="table-dark">
            <tr>
                <th>メールID</th>
                <th>メールタイトル</th>
                <th>メール本文</th>
                <th>送付アイテム</th>
                <th>個数</th>
            </tr>
            </thead>
            <tbody>
            @foreach($mails as $mail)
                <tr>
                    <td>{{$mail['id']}}</td>
                    <td>{{$mail['mail_title']}}</td>
                    <td>{{$mail['mail_message']}}</td>
                    <td>{{$mail['item_name']}}</td>
                    <td>{{$mail['item_num']}}</td>

                </tr>

        @endforeach
    </ul>
    
@endsection

