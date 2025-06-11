<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Contribution extends Model
{
    use HasFactory;

    protected $guarded = [
        'id',
    ];

    //リレーショナル（1対多）
    public function results()
    {
        return $this->hasMany(Result::class);
    }
}
