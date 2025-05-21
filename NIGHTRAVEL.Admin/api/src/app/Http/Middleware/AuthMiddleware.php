<?php

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;
use Symfony\Component\HttpFoundation\Response;

class AuthMiddleware
{
    /**
     * Handle an incoming request.
     *
     * @param \Closure(\Illuminate\Http\Request): (\Symfony\Component\HttpFoundation\Response) $next
     */
    public function handle(Request $request, Closure $next): Response
    {
        //セッションに指定したキーが存在するか
        if (!$request->session()->exists('login')) {
            return redirect()->route('login');
        }
        $response = $next($request);                         //ここでController実行
        return $response;
    }
}
