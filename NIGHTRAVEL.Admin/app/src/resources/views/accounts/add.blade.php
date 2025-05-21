<html lang="ja">
<body>
<h1>■新規登録</h1>
<form method="post" action="{{url('accounts/doAdd')}}">
    @csrf
    <label>ユーザー名：<input type="text" name="name"></label><br>
    <label>パスワード：<input type="password" name="pass"></label><br>
    <label>パスワード確認：<input type="password" name="pass_confirmation"></label><br>
    <label><input type="submit" value="登録する" name="btn_submit"></label>
    <input type="hidden" name="action" value="add">
    @if($errors->any())
        <ul>
            @foreach($errors->all() as $error)
                <li>{{$error}}</li>
            @endforeach
        </ul>
    @endif
</form>
</body>
</html>
