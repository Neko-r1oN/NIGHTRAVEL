<?php

namespace Database\Factories;

use App\Models\User;
use Illuminate\Database\Eloquent\Factories\Factory;
use function Symfony\Component\Translation\t;

/**
 * @extends \Illuminate\Database\Eloquent\Factories\Factory<\App\Models\User>
 */
class UserFactory extends Factory
{
    /**
     * Define the model's default state.
     *
     * @return array<string, mixed>
     */
    protected $model = User::class;

    public function definition(): array
    {
        //日付ランダム生成
        $scheduled_date = $this->faker->dateTimeBetween('+1day', '+1year');
        return [
            'name' => $this->faker->unique()->name,
            'level' => $this->faker->numberBetween(1, 99),//1～99のランダム
            'experience_point' => $this->faker->randomNumber(2),//5桁のランダム
            'life' => $this->faker->randomNumber(2),
            'created_at' => $scheduled_date->format('Y-m-d H:i:s'),
            'updated_at' => $scheduled_date->modify('+1hour')->format('Y-m-d H:i:s')
        ];
    }
}
