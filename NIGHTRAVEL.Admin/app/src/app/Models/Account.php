<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Account extends Model
{
    use HasFactory;

    //更新しないカラムを指定する
    protected $guarded = [                  //idはauto_incrementの為、指定しておく
        'id',
    ];
}
