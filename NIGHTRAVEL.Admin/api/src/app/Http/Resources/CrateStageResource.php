<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class CrateStageResource extends JsonResource
{
    /**
     * Transform the resource into an array.
     *
     * @return array<string, mixed>
     */
    public function toArray(Request $request): array
    {
        return [
            'create_stage_id' => $this->id,
            'stage_id' => $this->stage_id,
            'created_at' => $this->created_at->format('Y/m/d H:i:s')
        ];
    }
}
