# 📚 図書管理システム REST API(C# 開発演習 解答例)

C# による REST API 開発演習「図書管理システム」の模範解答です。**DDD(ドメイン駆動設計)** と **オニオンアーキテクチャ** に基づき、4層構成で実装しています。

---

## 概要

図書館の蔵書を管理する REST API です。図書の検索・登録・変更・削除といった基本操作に加え、ユーザー登録と JWT による認証(ログイン・ログアウト)を備えています。

学習教材として、各層の責務分担、依存性逆転、ユニットテストの書き方を、実際に動作するコードで示すことを目的としています。

---

## 技術スタック

| 分類 | 採用技術 |
|------|---------|
| 言語 / ランタイム | C# / .NET 10 (LTS) |
| Web フレームワーク | ASP.NET Core Web API |
| データベース | PostgreSQL |
| ORM | Entity Framework Core (Npgsql) |
| 認証 | JWT(HttpOnly Cookie に格納) |
| パスワードハッシュ | PBKDF2(`PasswordHasher`) |
| テスト | MSTest + Moq |
| API ドキュメント | Swagger (Swashbuckle) |
| 開発環境 | WSL2 (Ubuntu) + VS Code |

---

## アーキテクチャ

オニオンアーキテクチャに基づく4層構成です。依存は常に内側(ドメイン層)へ向かい、循環しません。

```
┌─────────────────────────────────────────────┐
│         Presentations(プレゼンテーション層)  │  Controller / ViewModel / Middleware / DI
│   ┌─────────────────────────────────────┐   │
│   │      Applications(アプリケーション層) │   │  UseCase / Service / DTO / UnitOfWork(I/F)
│   │   ┌─────────────────────────────┐   │   │
│   │   │       Domains(ドメイン層)    │   │   │  Entity / Repository(I/F) / DomainException
│   │   └─────────────────────────────┘   │   │
│   └─────────────────────────────────────┘   │
│         Infrastructures(インフラ層)          │  EF Core 実装 / Repository 実装 / UnitOfWork 実装
└─────────────────────────────────────────────┘
```

<!-- クラス図(任意): 4層の依存関係を表すパッケージ図。
     各プロジェクト(Domains / Applications / Infrastructures / Presentations)を
     パッケージとして表し、参照の矢印(下記「参照関係」に対応)を描くと、
     オニオンアーキテクチャの依存方向が一目で分かる。 -->

### 各層の責務

| 層 | プロジェクト | 責務 |
|----|------------|------|
| ドメイン層 | `LibraryApi.Domains` | エンティティ、リポジトリ/アダプタのインターフェース、ドメイン例外。他層に依存しない。 |
| アプリケーション層 | `LibraryApi.Applications` | ユースケース、サービス、DTO、Unit of Work のインターフェース、認証コンポーネント。 |
| インフラ層 | `LibraryApi.Infrastructures` | EF Core によるリポジトリ実装、アダプタ実装、Unit of Work 実装。 |
| プレゼンテーション層 | `LibraryApi.Presentations` | API コントローラ、ViewModel、例外ハンドリングミドルウェア、DI 登録。 |

### 参照関係

- **Applications → Domains**
- **Infrastructures → Domains, Applications**(`UnitOfWork` 実装が `IUnitOfWork` を実装するため)
- **Presentations → Domains, Applications, Infrastructures**

> Applications は Infrastructures を参照しません(循環参照を避けるため)。実装は DI で注入します。

---

## プロジェクト構成

```
LibraryApi/
├── LibraryApi.sln
├── Apps/
│   ├── LibraryApi.Domains/         ← ドメイン層
│   ├── LibraryApi.Applications/    ← アプリケーション層
│   ├── LibraryApi.Infrastructures/ ← インフラ層
│   └── LibraryApi.Presentations/   ← プレゼンテーション層(Web API)
└── Tests/
    ├── LibraryApi.Domains.Tests/
    ├── LibraryApi.Applications.Tests/
    ├── LibraryApi.Infrastructures.Tests/
    └── LibraryApi.Presentations.Tests/
```

---

## ドメインモデル

ドメイン層の中心となるエンティティと、その関係です。

- **Book**(集約ルート): 図書。`BookStock`(蔵書)を内包し、`Category`(分類)を参照する。
- **BookStock**: 蔵書数。`Book` に内包される。
- **Category**: 分類。
- **User**: 利用者。ユーザー名とハッシュ化済みパスワードを持つ。

<!-- クラス図(推奨): ドメインモデルのクラス図。
     Book(集約ルート)が BookStock を「内包(コンポジション)」し、
     Category を「参照(関連)」する関係を示す。User は独立。
     各クラスの主な属性(BookUuid / Title / Author、StockUuid / Stock、
     CategoryUuid / Name、UserUuid / Username / HashedPassword)と、
     ファクトリメソッド(Create / Restore)を載せると、設計意図が伝わる。 -->

---

## API エンドポイント

ベース URL は `https://localhost:<port>/library/api` です。

| # | メソッド | パス | 説明 | 認証 | 成功時 |
|---|---------|------|------|------|--------|
| 1 | GET | `/categories` | 分類一覧取得 | 要 | 200 |
| 2 | GET | `/books?keyword=` | 図書検索(書名の部分一致) | 要 | 200 |
| 3 | GET | `/books/{bookId}` | 図書詳細取得 | 要 | 200 |
| 4 | POST | `/books` | 図書登録 | 要 | 201 |
| 5 | PUT | `/books/{bookId}` | 図書変更 | 要 | 200 |
| 6 | DELETE | `/books/{bookId}` | 図書削除 | 要 | 204 |
| 7 | POST | `/users` | ユーザー登録 | 不要 | 201 |
| 8 | POST | `/auth/login` | ログイン(JWT を Cookie に発行) | 不要 | 200 |
| 9 | POST | `/auth/logout` | ログアウト(Cookie 削除) | 要 | 200 |

### エラーレスポンス形式

すべてのエラーは、以下の統一形式(camelCase)で返します。

```json
{
  "error": "BookNotFound",
  "message": "指定された図書が存在しません。"
}
```

| エラーコード | HTTP | 発生条件 |
|------------|------|---------|
| `ValidationError` | 400 | 入力値の検証エラー(ドメイン制約違反など) |
| `CategoryNotFound` | 400 | 図書登録時、指定分類が存在しない |
| `BookNotFound` | 404 | 指定図書が存在しない |
| `DuplicateUsername` | 409 | ユーザー名が既に使用されている |
| `AuthenticationFailed` | 401 | ログイン失敗(ユーザー名・パスワード不一致) |
| `Unauthorized` | 401 | 未認証で保護リソースにアクセス |
| `InternalServerError` | 500 | 想定外のサーバーエラー |

---

## レイヤー間のデータの流れ

リクエストは、各層を通過しながら型を変換していきます。

```
HTTP Request
   │  ViewModel(Request)
   ▼
Controller ──[Adapter]── DTO ──▶ UseCase
                                   │
                                   ├── Service ──▶ Repository(I/F)
                                   │                   │
                                   │            [EF Core 実装]
                                   │                   ▼
                                   │              PostgreSQL
                                   ▼
                          Domain Entity
   ┌───────────────────────────────┘
   ▼
DTO ──[Adapter]── ViewModel(Response) ──▶ HTTP Response
```

変換はすべて共通インターフェース `IAdapter<TLeft, TRight>` に統一しています(`Convert` / `Restore` の双方向)。

<!-- クラス図(任意): IAdapter インターフェースと、その実装クラス群の関係図。
     IAdapter<TLeft, TRight> を中心に、各層の Adapter
     (CategoryDtoAdapter, BookResponseAdapter, RegisterBookRequestAdapter など)が
     これを実装する構造を示すと、「変換の統一」という設計が伝わる。
     あわせて Convert / Restore の方向(レスポンス用は Convert、
     リクエスト用は Restore を実装)を注記するとよい。 -->

---

## 主な設計判断

### ドメインモデル
- エンティティの同一性は **UUID** で表し、値オブジェクトは使用しない。
- 生成はファクトリメソッド(`Create` = 新規・UUID 採番、`Restore` = 復元)に集約。
- 不変条件の検証はドメイン内で行い、違反時は `DomainException` を送出。
- 集約ルートは `Book`(蔵書 `BookStock` を内包、`Category` を参照)。

### 層をまたぐ変換(Adapter)
- 各層の境界での変換は、共通の `IAdapter<TLeft, TRight>` で統一(`Convert` / `Restore` の双方向)。
- ViewModel 変換 Adapter は型引数を「DTO が左、ViewModel が右」で統一。

### 例外と HTTP の対応
- ユースケースは業務的な例外(`NotFoundException` / `InvalidInputException` / `ConflictException` / `AuthenticationException`)を送出し、**HTTP ステータスへの変換はプレゼンテーション層のミドルウェアが担う**。
- 例外はエラーコードを保持し、ミドルウェアがそれを用いて統一形式で応答する。

<!-- クラス図(任意): 例外クラスの階層図。
     System.Exception を基底に、NotFoundException / InvalidInputException /
     ConflictException / AuthenticationException(各 ErrorCode を保持)と、
     ドメイン層の DomainException を並べ、各例外が
     どの HTTP ステータスに変換されるか(404 / 400 / 409 / 401)を注記すると、
     エラーハンドリングの全体像が伝わる。 -->

### トランザクション
- 書き込み系ユースケースは `IUnitOfWork` でトランザクション境界を制御。
- 図書と蔵書は同一トランザクションで永続化・削除される。

### 認証
- ログイン成功時、JWT を **HttpOnly Cookie**(`access_token`)に格納。
- 認証の検証はミドルウェア(JWT Bearer)が Cookie からトークンを読んで行う。
- 認証失敗はユーザー名不在とパスワード不一致を区別しない(列挙攻撃対策)。

<!-- シーケンス図(任意): ログイン〜保護リソースアクセスの流れ。
     ① POST /auth/login でユーザー検証 → JWT 発行 → Set-Cookie
     ② 以降のリクエストで Cookie 送信 → ミドルウェアがトークン検証 → 認可
     ③ POST /auth/logout で Cookie 削除
     という流れをシーケンス図で示すと、認証の仕組みが理解しやすい。
     ※クラス図ではなくシーケンス図が適する箇所。 -->

---

## テスト

各層をユニットテストで検証しています(MSTest + Moq)。「ひとつ下の層をモック化する」方針です。

| 対象 | 内容 |
|------|------|
| ドメイン層 | エンティティの生成・検証ロジック |
| インフラ層 | リポジトリの統合テスト(実 DB)、Adapter |
| アプリケーション層 | Service、UseCase(Service・UnitOfWork をモック)、認証コンポーネント |
| プレゼンテーション層 | Adapter、Controller(UseCase をモック)、例外ハンドリングミドルウェア |

```bash
dotnet test
```

---

## セットアップ

### 前提

- .NET 10 SDK
- PostgreSQL
- (Windows の場合)WSL2 + Ubuntu

### 手順

1. リポジトリを取得

```bash
   git clone <リポジトリURL>
   cd LibraryApi
```

2. データベースを構築(別紙「データベース構築手順」に従い、`library_db` とテーブル・初期データを作成)

3. 接続文字列と JWT 設定を確認(`Apps/LibraryApi.Presentations/appsettings.json`)

```json
   {
     "ConnectionStrings": {
       "LibraryDb": "Host=localhost;Port=5432;Database=library_db;Username=postgres;Password=<パスワード>"
     },
     "Jwt": {
       "Issuer": "LibraryApi",
       "Audience": "LibraryApiUsers",
       "SecretKey": "<32文字以上の秘密鍵>",
       "ExpiresInMinutes": 60
     }
   }
```

4. ビルドと起動

```bash
   dotnet build
   dotnet run --project Apps/LibraryApi.Presentations
```

5. ブラウザで Swagger UI を開く

```
   http://localhost:<port>/swagger
```

> **注意:** `appsettings.json` の `SecretKey` や接続パスワードは開発用の値です。本番環境では環境変数やシークレットマネージャで管理してください。また Cookie の `Secure` 属性は、本番では `true` にしてください。

---

## 関連ドキュメント

- プロジェクト作成手順(`<パスを記載>`)
- データベース構築手順(`<パスを記載>`)

---

## ライセンス / 著作

© 2026 Fullness, Inc. All Rights Reserved.
