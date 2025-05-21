<!DOCTYPE html>
<html lang="ja">
<head>
    <title>@yield('title')</title>
    <link href="/css/bootstrap.min.css" rel="stylesheet"
          integrity="sha384-9ndCyUaIbzAi2FUVXJi0CjmCapSmO7SnpJef0486qhLnuZ2cdeRhO02iuK6FUUVM" crossorigin="anonymous">
</head>
<body>
<div class="container">
    <header
        class="d-flex flex-wrap align-items-center justify-content-center justify-content-md-between py-3 mb-4 border-bottom">
        <div class="col-md-3 mb-2 mb-md-0">
            <a href="index.blade.php" class="d-inline-flex link-body-emphasis text-decoration-none">
                <svg class="bi" width="40" height="32" role="img" aria-label="Bootstrap">
                    <use xlink:href="#bootstrap"></use>
                </svg>
            </a>
        </div>

        <ul class="nav col-12 col-md-auto mb-2 justify-content-center mb-md-0">
            <li><a href="{{route('accounts.index')}}" class="nav-link px-2 @yield('accounts')">アカウント一覧</a></li>
            <li><a href="{{route('accounts.create')}}" class="nav-link px-2">アカウント登録</a></li>
            <li><a href="{{route('accounts.mailList')}}" class="nav-link px-2" @yield('mail')>メールマスタデータ</a>
            </li>
            <li><a href="{{route('accounts.user_mail')}}"
                   class="nav-link px-2" @yield('user_mail')>ユーザー受信メール一覧</a>
            </li>
            <li><a href="{{route('accounts.sendmail')}}"
                   class="nav-link px-2" @yield('send_mail')>メール送信</a>
            </li>
            <li><a href="{{route('accounts.user-list')}}" class="nav-link px-2 @yield('users')">ユーザー一覧</a></li>
            <li><a href="{{route('accounts.item')}}" class="nav-link px-2 @yield('items')">アイテム一覧</a></li>
            <li><a href="{{route('accounts.have-item')}}" class="nav-link px-2 @yield('haveItems')">所持アイテム一覧</a>
            </li>
            <li><a href="{{route('accounts.have-item')}}" class="nav-link px-2 @yield('itemLogs')">アイテムログ</a>
            </li>
            <li><a href="{{route('accounts.followList')}}" class="nav-link px-2 @yield('follow')">フォロー一覧</a>
            </li>
        </ul>

        <div class="col-md-3 text-end">
            <form method="post" action="{{route('doLogout')}}">
                @csrf
                <button type="submit" class="btn btn-outline-primary me-2">ログアウト</button>
                <input type="hidden" name="action" value="doLogout">
            </form>
        </div>
    </header>
</div>
@yield('body')
<script src="/js/bootstrap.min.css"
        integrity="sha384-geWF76RCwLtnZ8qwWowPQNguL3RmwHVBC9FhGdlKrxdiJJigb/j/68SIy3Te4Bkz"
        crossorigin="anonymous"></script>
</body>
</html>
