@extends('layouts')
@section('title','ステージ一覧')
@section('body')

    <ul>
        <ul class="nav col-18 col-md-auto mb-2 justify-content-center mb-md-0">
            <li><a href="accounts" class="nav-link ">account</a></li>
            <li><a href="stages" class="nav-link px-2 link-secondary">stages</a></li>
        </ul>
       

        <table class="table">
            <thead class="table-dark">
            <tr>
                <th>ステージID</th>
                <th>ステージ名</th>

            </tr>
            </thead>
            <tbody>
            @foreach($stages as $stage)
                <tr>
                    <th>{{$stage['id']}}</th>
                    <th>{{$stage['stage_name']}}</th>

                </tr>
        @endforeach
    </ul>

@endsection
