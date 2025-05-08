@extends('layouts')
@section('title','アカウント登録')
@section('body')
    <form class="create h3   font-weight-normal nav col-18 col-md-auto mb-2 justify-content-center mb-md-0"
          method="POST"
          action="{{url('accounts/store')}}">
        @csrf
        <div class="container">
            <div class="row">
                

                <input type="text" name="name" id="name" class="form-control" placeholder="ユーザー名" required="">

                <input type="password" name="password" id="password" class="form-control" placeholder="パスワード"
                       required="">
                <input type="password" name="password2" id="password2" class="form-control" placeholder="パスワード(確認用)"
                       required="">


                <label><input hidden="hidden" name="action" value="store"></label>

                <button class="btn btn-lg btn-primary btn-block" type="submit" name="action">登録</button>


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
@endsection
