# TodoApi Ver4 - JWT認証・Docker化

C#学習プロジェクト。Ver3（レイヤー分離・例外処理）をベースに、JWT認証とDockerを追加した。

## 学習ロードマップ上の位置づけ

```
Ver1（インメモリ）→ Ver2（SQLite・Swagger）→ Ver3（レイヤー分離・例外処理）→ Ver4（JWT認証・Docker）
```

---

## 実装内容

### JWT認証
- `/register` でユーザー登録（BCryptでパスワードハッシュ化）
- `/login` でJWTトークン発行
- Todo系エンドポイントはすべて `RequireAuthorization()` で保護
- トークンの検証は `TokenValidationParameters` で設定

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("シークレットキー"))
        };
    });
```

### EF Core + SQLite
- `DbContext` でマイグレーション管理
- 起動時に `db.Database.Migrate()` を呼び出してDB自動作成

### レイヤー構成

```
Program.cs（エンドポイント定義）
  └─ Service層（ITodoService / IAuthService）
       └─ Repository層（ITodoRepository）
            └─ DbContext（EF Core）
```

### Swagger UI（SwaggerGen）
- Bearer認証のセキュリティ定義を追加
- Authorize ボタンからJWTトークンをセットして認証済みリクエストを送れる

---

## Docker化

### Dockerfile（マルチステージビルド）

```
build ステージ  → dotnet restore / build / publish
final ステージ  → publishされた成果物だけをコピーして軽量イメージに
```

### docker-compose.yml

```yaml
services:
  api:
    build: .
    ports:
      - "8080:8080"
    volumes:
      - ./todos.db:/app/todos.db
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

- `volumes` でホスト側の `todos.db` をコンテナにマウント → コンテナを再起動してもDBが消えない
- `ports: "8080:8080"` → ホストの8080をコンテナの8080に転送

### 起動コマンド

```bash
docker compose up --build
```

動作確認は `http://localhost:8080/swagger`

---

## 詰まったポイントと解決策

### .NET10 と Swashbuckle（Swagger）の非互換問題

**症状**：`docker compose up --build` が通っても Swagger UI が開かない

**原因**：.NET10 に対して Swashbuckle.AspNetCore が未対応だった

**解決策**：Swashbuckle をそのまま使い続けつつ、`AddSwaggerGen` の設定を見直して動作確認。
本来は Scalar への移行も選択肢としてあったが、今回は既存構成のまま完成させることを優先した。

---

## エンドポイント一覧

| メソッド | パス | 認証 | 説明 |
|--------|------|------|------|
| POST | /register | 不要 | ユーザー登録 |
| POST | /login | 不要 | JWTトークン取得 |
| GET | /todos | 必要 | Todo一覧取得 |
| POST | /todos | 必要 | Todo作成 |
| PUT | /todos/{id} | 必要 | Todo更新 |
| DELETE | /todos/{id} | 必要 | Todo削除 |

---

## 次のステップ

- Python + FastAPI で同等のTodoApiを実装
- AWS CLF 取得
