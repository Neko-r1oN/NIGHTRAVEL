@extends('layouts')
@section('title','メール送信')
@section('body')

    <div class="container">
        <div class="row">
            

            <form method="post" action="{{route('sent')}}">
                @csrf
                <input class="form-control" type="text" name="user_id" id="user_id"
                       placeholder="送信するユーザーID"><br>
                <input class="form-control" type="text" name="mail_id" id="mail_id" placeholder="送信するメールID"><br>
                <label for="button">
                    <button type="submit" class="btn btn-outline-primary me-2">送信する</button>
                </label>
            </form>

            <div class="col-md-3 text-end">
                <form method="get" action="{{route('users.index')}}">
                    @csrf
                    <button type="submit" class="btn btn-outline-primary me-2">もどる</button>
                </form>

            </div>

        </div>
    </div>

@endsection
