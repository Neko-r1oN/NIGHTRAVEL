<html lang="ja">
<body>
<h1>■アカウント更新</h1>
<form method="post" action="{{route('accounts.doUpdate')}}">
    @csrf
    <label>パスワード：<input type="text" name="pass"></label><br>
    <label>パスワード確認：<input type="text" name="pass_confirmation"></label><br>
    <label><input type="submit" value="更新" name="btn_submit"></label>
    <input type="hidden" name="id" value="{{$id}}">
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
