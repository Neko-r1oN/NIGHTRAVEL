<html lang="ja">
<body>
<h1>■ログイン</h1>
<form method="post" action="{{route('doLogin')}}">
    @csrf
    <label>ユーザー名：<input type="text" name="name"></label><br>
    <label>パスワード：<input type="password" name="pass"></label><br>
    <label><input type="submit" value="ログイン" name="btn_submit"></label>
    <input type="hidden" name="action" value="doLogin">
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
