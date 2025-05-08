<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <title class="col-md-10 col-md-offset-1">@yield('title')</title>
    <link href="/css/bootstrap.min.css" rel="stylesheet"
          integrity="sha384-9ndCyUaIbzAi2FUVXJi0CjmCapSmO7SnpJef0486qhLnuZ2cdeRhO02iuK6FUUVM" crossorigin="anonymous">

    <!-- CSSの設定ファイル -->
    <link rel="stylesheet" href="sidebars.css">
</head>
<body>
<h1 class="text">ユーザー情報更新</h1>
<div class="container">
    <header
        class="d-flex flex-wrap align-items-center justify-content-center justify-content-md-between py-3 mb-4 border-bottom">

        <ul class="nav col-12 col-md-auto mb-2 justify-content-center mb-md-0">


        </ul>
    </header>
</div>

<form class="form-signin" method="POST" action="{{route('accounts.update',['id' => $account['id']])}}">
    @csrf

    @if($errors->any())
        <ul>
            @foreach($errors->all() as $error)
                <li>{{$error}}</li>
            @endforeach
        </ul>
    @endif

    <label for="inputEmail" class="sr-only">アカウント名</label>
    <input type="text" id="inputEmail" name="name" class="form-control" value="{{$account['name']}}" disabled>
    <label for="inputPassword" class="sr-only">パスワード</label>
    <input type="password" id="password" name="password" class="form-control" placeholder="パスワード" required>

    <div class="checkbox mb-3 justify-content-center">
    </div>
    <button class="btn btn-lg btn-primary btn-block" name="register_btn" type="submit">更新</button>

</form>
<script src="/js/bootstrap.bundle.min.js"
        integrity="sha384-q8i/X+965DzO0rT7abK41JStQIAqVgRVzpbzo5smXKp4YfRvH+8abtTE1Pi6jizo"
        crossorigin="anonymous"></script>
</body>
</html>
