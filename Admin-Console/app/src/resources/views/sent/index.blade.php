@extends('layouts')
@section('title','メール送信')
@section('body')
    <div class="container">
        <div class="row">
            <div class="col-md-6">
                <h1>メールが送信されました</h1>


            </div>
        </div>
        <form method="get" action="{{route('index')}}">
            <button class="btn btn-lg btn-primary btn-block" type="submit" name="action">もどる</button>
        </form>
    </div>

@endsection
