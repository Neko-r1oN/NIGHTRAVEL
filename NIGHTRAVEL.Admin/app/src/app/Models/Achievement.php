<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Achievement extends Model
{
    use HasFactory;

    protected $guarded = [
        'id',
    ];

    //リレーショナル（1対多）
    public function achievement_statuses()
    {
        return $this->hasMany(AchievementStatus::class);
    }
    
}
