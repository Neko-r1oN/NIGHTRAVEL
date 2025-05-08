<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <title>@yield('title')</title>
    <link href="/css/bootstrap.min.css" rel="stylesheet"
          integrity="sha384-9ndCyUaIbzAi2FUVXJi0CjmCapSmO7SnpJef0486qhLnuZ2cdeRhO02iuK6FUUVM" crossorigin="anonymous">

    <!-- CSSの設定ファイル -->
    <link rel="stylesheet" href="sidebars.css">
</head>
<body>
<h1>ログイン</h1>
<?php
    //
    //ログインページ表示
    //2024/07/01 川口京佑
    //
?>

<form class=" h3   font-weight-normal nav col-18 col-md-auto mb-2 justify-content-center mb-md-0" method="POST"
      action="{{url('auth/doLogin')}}">
    @csrf
    <div class="container">
        <div class="row">


            <input type="text" name="name" id="inputPassword" class="form-control" placeholder="Name"
                   required="">

            <input type="password" name="password" id="inputPassword" class="form-control" placeholder="Password"
                   required="">


            <label><input hidden="hidden" name="action" value="doLogin"></label>

            <button class="btn btn-lg btn-primary btn-block" type="submit" name="action">Sign in</button>
            @if(isset($error))
                <br>
                <div>{{$error}}</div>
                <br>
            @endif
        </div>
    </div>

    @if($errors -> any())
        <ul>
            @foreach($errors->all() as $error)
                <li>{{$error}}</li>
            @endforeach
        </ul>
    @endif

</form>
</body>
<script src="/js/bootstrap.bundle.min.js"
        integrity="sha384-q8i/X+965DzO0rT7abK41JStQIAqVgRVzpbzo5smXKp4YfRvH+8abtTE1Pi6jizo"
        crossorigin="anonymous"></script>

</html>
