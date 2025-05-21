<?php

namespace App\Http\Controllers;

use App\Models\CreateStage;
use App\Http\Resources\CrateStageResource;
use App\Http\Resources\CrateStagePositionResource;
use App\Models\CreateStagePosition;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\DB;
use Mockery\Exception;

class CreateStageController extends Controller
{
    //クリエイトステージ情報一覧取得
    public function index()
    {
        $create_stages = CreateStage::all();

        return response()->json(
            CrateStageResource::collection($create_stages)
        );
    }

    //ステージクリエイト情報取得
    public function show(Request $request)
    {
        $create_stage = CreateStage::findOrFail($request->create_stage_id);
        $create_stage_position = CreateStagePosition::where("create_stage_id", "=", $request->create_stage_id)->get();

        $create_stage["pos_list"] = $create_stage_position;

        return response()->json($create_stage);
    }

    //ステージクリエイト情報登録
    public function store(Request $request)
    {
        try {
            //トランザクション処理
            $create = DB::transaction(function () use ($request) {
                $create_stage = CreateStage::create([
                    "stage_id" => $request->stage_id
                ]);
                for ($i = 0; $i < count($request->pos_list); $i++) {
                    $create = CreateStagePosition::create([
                        'create_stage_id' => $create_stage->id,
                        'type' => $request->pos_list[$i]["type"],
                        'x' => $request->pos_list[$i]["x"],
                        'y' => $request->pos_list[$i]["y"]
                    ]);
                }
                return $create;
            });
            return response()->json();
        } catch (Exception $e) {
            return response()->json($e, 500);
        }
    }
}
