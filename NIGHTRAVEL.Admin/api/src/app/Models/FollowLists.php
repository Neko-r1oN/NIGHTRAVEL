<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class FollowLists extends Model
{
    use HasFactory;

    protected $guarded = [
        'id',
    ];

    public function followUser()
    {
        return $this->hasOne(User::class, 'id', 'follow_user');
    }

    public function user()
    {
        return $this->hasOne(User::class, 'id', 'user_id');
    }
}
