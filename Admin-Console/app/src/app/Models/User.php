<?php

    namespace App\Models;

    use Illuminate\Database\Eloquent\Factories\HasFactory;
    use Illuminate\Database\Eloquent\Model;
    use Items;

    class User extends Model
    {
        use HasFactory;

        protected $guarded = [
            'id',
        ];

        public function items()
        {
            return $this->belongsToMany(
                items::class, 'id', 'name')
                ->withPivot('amount');
        }
    }
