<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Stage extends Model
{
    use HasFactory;

    protected $guarded = [
        'id',
    ];

    //リレーショナル（1対多）
    public function enemies()
    {
        return $this->hasMany(Enemy::class);
    }

    public function results()
    {
        return $this->hasMany(Result::class);
    }
}
