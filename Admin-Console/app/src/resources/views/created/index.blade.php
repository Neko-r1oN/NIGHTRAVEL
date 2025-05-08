@extends('layouts')
@section('title','アカウント登録')
@section('body')

    <form class=" h3   font-weight-normal nav col-18 col-md-auto mb-2 justify-content-center mb-md-0" method="get"
          action="{{url('accounts.index')}}">
        @csrf
        <div class="container">
            <div class="row">
                

                <h1>登録完了しました</h1>
            </div>
        </div>
    </form>
    <form method="get" action="{{route('index')}}">
        <button class="btn btn-lg btn-primary btn-block" type="submit" name="action">もどる</button>
    </form>
@endsection
