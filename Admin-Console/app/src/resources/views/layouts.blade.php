<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <title class="col-md-10 col-md-offset-1">@yield('title')</title>
    <link href="/css/bootstrap.min.css" rel="stylesheet"
          integrity="sha384-9ndCyUaIbzAi2FUVXJi0CjmCapSmO7SnpJef0486qhLnuZ2cdeRhO02iuK6FUUVM" crossorigin="anonymous">

    <!-- CSSの設定ファイル -->

</head>
<body>

<h1 class="text">@yield('title')</h1>
<div class="container d-flex">
    <div class="d-flex align-items-center sidebar_fixed center-block">
        <div class="d-flex flex-column flex-shrink-0 p-3 bg-body-tertiary" style="width: 280px;">

            <a
                class="d-flex align-items-center mb-3 mb-md-0 me-md-auto link-body-emphasis text-decoration-none">
                <svg class="bi me-2" width="40" height="32">
                    <use xlink:href="#bootstrap"></use>
                </svg>
                <span class="fs-4">マスターデータ</span>
            </a>
            <hr>
            <ul class="nav nav-pills flex-column mb-auto">
                <li class="nav-item">
                    <a href="{{ route('index') }}" class="nav-link " aria-current="page">
                        <svg class="bi me-2" width="16" height="16">
                            <use xlink:href="#home"></use>
                        </svg>
                        管理者アカウント
                    </a>
                </li>
                <li>
                    <a href="{{ route('stages.index') }}" class="nav-link">
                        <svg class="bi me-2" width="16" height="16">
                            <use xlink:href="#table"></use>
                        </svg>
                        ステージリスト
                    </a>
                </li>

                <a
                    class="d-flex align-items-center mb-3 mb-md-0 me-md-auto link-body-emphasis text-decoration-none">
                    <svg class="bi me-2" width="40" height="32">
                        <use xlink:href="#bootstrap"></use>
                    </svg>
                    <span class="fs-4">ユーザーデータ</span>
                </a>
                <hr>
                <li>
                    <a href="{{ route('users.index') }}" class="nav-link">
                        <svg class="bi me-2" width="16" height="16">
                            <use xlink:href="#grid"></use>
                        </svg>
                        ユーザーリスト
                    </a>
                </li>
                <li>
                    <a href="{{ route('scoreLogs.index') }}" class="nav-link">
                        <svg class="bi me-2" width="16" height="16">
                            <use xlink:href="#table"></use>
                        </svg>
                        スコアログ
                    </a>
                </li>
                <hr>
                <li>
                    <form method="POST" action="{{url('auth/doLogout')}}" class="nav-link ">
                        <a class="nav-link link-body-emphasis">
                            @csrf
                            <button type="submit" class="nav-link link-body-emphasis">ログアウト</button>
                        </a>
                    </form>
                </li>
            </ul>
        </div>
    </div>
    @csrf
    @yield('body')
</div>
@yield('update')
</body>

<script src="/js/bootstrap.bundle.min.js"
        integrity="sha384-q8i/X+965DzO0rT7abK41JStQIAqVgRVzpbzo5smXKp4YfRvH+8abtTE1Pi6jizo"
        crossorigin="anonymous"></script>

</html>
