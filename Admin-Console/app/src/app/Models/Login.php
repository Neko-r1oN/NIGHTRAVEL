<?php

    //
    //ログインクラス
    //2024/05/28 川口京佑
    //

    namespace App\Models;

    //use Config\DataBase;

    class Login
    {
        private $db;

        public function __construct()
        {
            //$this->db = new Database();
        }

        // ログインページのバリデーション
        public function validation($request)
        {
            $errors = [];

            // 入力チェック
            if (empty($request['name'])) {
                $errors[] = "[ユーザー名]は必須です。";
            }
            if (empty($request['password'])) {
                $errors[] = "[パスワード]は必須です。";
            }
            if (!empty($request['name']) && !empty($request['password'])) {
                if (!$this->loginCheck($request['name'], $request['password'])) {
                    $errors[] = "[ユーザー名]または[パスワード]に誤りがあります。";
                }
            }

            return $errors;
        }

        // DBに接続して指定した名前とパスワードでログインできるかどうか
        public function loginCheck($username, $password)
        {
            // パラメータとの紐づけ
            $params = [
                ':name' => $username
            ];

            // DBに接続,SQL文実行
            //$stmt = $this->db->getConnection()->prepare("select password from users where name = :name");
            //$stmt->execute($params);
            //$result = $stmt->fetch();

            if (empty($result)) {
                echo '通信に失敗しました。';
                return null;
            } else {
                return password_verify($password, $result['password']);
            }
        }
    }
