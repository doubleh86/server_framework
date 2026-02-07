# ServerFramework 포트폴리오 (간단 분석)

> **요약**  
> `ServerFramework`는 실서비스(특히 게임/실시간 서버)에서 반복되는 **시간/설정/로그/DB/Redis/분산락** 인프라를 C#/.NET 라이브러리 형태로 모듈화한 프로젝트입니다.  
> 목표는 “서버 앱을 만들 때 매번 복붙하던 기반 코드”를 표준화해 **재사용성과 운영 안정성**을 높이는 것입니다.

---

## 1) 프로젝트 한눈에 보기

- **프로젝트 타입**: .NET 라이브러리
- **Target Framework**: `net10.0`
- **구성 요약**
    - `CommonUtils`: 시간/설정/로그/ID/비밀번호 해시 등 공통 유틸
    - `RedisService`: Redis 연결 관리/구독/서비스 선택(샤딩) + RedLock 분산락
    - `SqlServerServices`: EF Core 기반 DBContext 베이스 + DB 예외/에러 코드
    - `MySqlServices`: Dapper 기반 MySQL 접근 베이스
    - `GrpcServices`: gRPC 서비스 베이스(확장 포인트)

---

## 2) 핵심 구현 포인트 (Highlights)

### A. 서버 시간 표준화 (TimeZoneHelper)
- 서버 시간 기준을 `TimeZoneHelper.Initialize(timeZoneId)`로 고정
- `DateTime.UtcNow` 대신 `TimeZoneHelper.UtcNow / ServerTimeNow` 사용을 유도
- `IDateTimeProvider` 구조로 시간 소스를 교체 가능(테스트/이벤트 시뮬레이션에 유리)

### B. 설정 로딩 (ConfigurationHelper)
- 여러 JSON 설정 파일을 순서대로 로딩해 환경별 설정 분리 가능
- `GetSection<T>()`로 Strongly-typed 설정 바인딩 제공

### C. 로깅 표준화 (LoggerService + Serilog)
- `IConfiguration` 기반으로 Serilog 설정을 읽어 로거 생성
- 호출부는 `Information/Warning/Error/Debug`로 단순화

### D. Redis 인프라 (연결 유지/구독/샤딩)
- `RedisServiceBaseAzure`:
    - Redis 연결을 유지하고 예외 발생 시 재연결/재시도 로직 포함
    - String Get/Set, Publish/Subscribe, Lua 실행 등 기본 기능 제공
- `RedisServiceManager`:
    - 서비스 타입별로 여러 Redis 연결을 구성하고 관리
    - `accountId` 기반으로 특정 인스턴스를 선택(간단한 샤딩 전략)
    - Subscribe는 첫 번째 연결 사용 + `StartSubscribe()`로 일괄 시작

### E. 분산락 (RedLockManager)
- 여러 Redis 엔드포인트 기반 RedLock 구성
- `CreateLock / CreateLockAsync`로 동시 작업 제어(멀티 서버 환경 대응)
- 리소스 키를 `RLOCK_{accountId}` 형태로 표준화

### F. DB 접근 베이스 (EF Core + Dapper)
- `SqlServerServiceBase (DbContext)`:
    - 설정 기반으로 SQL Server 또는 MySQL(EF Core Pomelo) 연결 구성
    - Lazy loading 옵션 제공
- `MySqlDapperServiceBase`:
    - Dapper 기반 쿼리 실행 래핑
    - 모델 매핑/동적 조회 API 제공
- `DatabaseException + ServerError`:
    - 에러 코드(enum)와 호출 위치 정보를 포함해 디버깅 효율 강화

---

## 3) 사용 기술 / 라이브러리

- **DB**: EF Core, Dapper, Microsoft.Data.SqlClient, Pomelo.EntityFrameworkCore.MySql, MySqlConnector
- **Redis**: StackExchange.Redis, RedLock.net
- **Logging**: Serilog (+ Console/File/Configuration)
- **RPC**: gRPC(Core/Client)

---

## 4) 사용예제

- WEB 기반 게임 서버 : https://github.com/doubleh86/game_server_v2
- 실시간 서버 : https://github.com/doubleh86/no-name-server

## 5) 다음 개선 아이디어 (짧게)

- Redis DB 매핑(`typeName -> dbId`)을 하드코딩 대신 설정 기반으로 전환
- 설정 샘플(`appsettings.json`)과 초기화 예제를 함께 제공해 “바로 사용 가능” 수준으로 문서 강화
- gRPC 베이스 사용 예제(클라이언트/서버) 추가