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

    //リレーショナル（多対多）
    public function items()
    {
        return $this->belongsToMany(
            Item::class, 'have_items', 'user_id', 'item_id')
            ->withPivot('possession');
    }
}
