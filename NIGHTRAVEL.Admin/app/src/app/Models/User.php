<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class User extends Model
{
    use HasFactory;

    protected $guarded = [
        'id',
    ];

    //リレーショナル（1対多）
    public function user_relics()
    {
        return $this->hasMany(UserRelic::class);
    }

    public function achivement_status()
    {
        return $this->hasMany(AchievementStatus::class);
    }

    public function results()
    {
        return $this->hasMany(Result::class);
    }
}
