<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Weapon extends Model
{
    use HasFactory;

    //更新しないカラムを指定する
    protected $guarded = [                  //idはauto_incrementの為、指定しておく
        'id',
    ];

    //リレーショナル（1対多）
    public function status_enhancements()
    {
        return $this->hasMany(StatusEnhancement::class);
    }

    public function results()
    {
        return $this->hasMany(Result::class);
    }
}
