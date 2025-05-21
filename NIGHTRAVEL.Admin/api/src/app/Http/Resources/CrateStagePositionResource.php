<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class CrateStagePositionResource extends JsonResource
{
    /**
     * Transform the resource into an array.
     *
     * @return array<string, mixed>
     */
    public function toArray(Request $request): array
    {
        return [
            'create_stage_id' => $this->create_stage_id,
            'type' => $this->type,
            'x' => $this->x,
            'y' => $this->y,
            'created_at' => $this->created_at->format('Y/m/d H:i:s')
        ];
    }
}
