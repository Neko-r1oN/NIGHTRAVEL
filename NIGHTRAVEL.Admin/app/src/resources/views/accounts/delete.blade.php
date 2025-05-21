<html lang="ja">
<body>
<h1>■アカウント削除</h1>
<p>アカウント削除しますか？</p>
<form method="post" action="{{route('accounts.doDelete')}}">
    @csrf
    <button type="submit">削除する</button>
    <input type="hidden" name="id" value="{{$id}}">
</form>
</body>
</html>
