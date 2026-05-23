# Parking API – projekt zaliczeniowy z laboratoriów ASP.NET

Backendowy projekt zaliczeniowy realizujący całość wymagań z laboratoriów oraz indywidualne **Zadanie 23** (API pracownika parkingu). Zbudowany jako modularny monolit w architekturze czystej — z osobnymi warstwami domeny, infrastruktury, prezentacji (REST API) i testów.

## Autor

- **Kamil Gębala**

## Repozytorium

- GitHub: `https://github.com/kamilgebala/ASP.git`

## Zrealizowane funkcje

Backend systemu parkingu zrealizowany w **czystej architekturze** (`CoreApp` / `Infrastructure` / `WebApi` / `UnitTest`) z wykorzystaniem wzorców **Repository + Unit of Work + DTO + Service**, w pełni przykryty testami.

- **Model domenowy parkingu** – bramki wjazdowe i wyjazdowe (`ParkingGate`), pojazdy (`Vehicle`), sesje parkingowe (`ParkingSession`), taryfy cenowe (`ParkingTariff`), zdjęcia z kamer (`CameraCapture`) oraz konta użytkowników i refresh tokeny do JWT.
- **Persystencja Entity Framework Core + SQLite** – `ParkingDbContext` (Identity + dane domenowe), kompletny zestaw repozytoriów EF, jednostka pracy (`EfParkingUnitOfWork`) i migracje (`InitialCreate`, `AddRefreshTokens`, `AddCreatedBy`).
- **Alternatywne repozytoria in-memory** – dla niezależnych testów jednostkowych logiki serwisów bez konieczności podnoszenia bazy.
- **CRUD bramek parkingowych z paginacją** – tworzenie, edycja, zmiana statusu operacyjnego (PATCH) i pobieranie z paged result.
- **Zdjęcia z kamer (captures)** – dodawanie, listowanie i usuwanie zdjęć przypisanych do bramki, z mapowaniem encja → DTO.
- **API pracownika parkingu** – podgląd aktywnie zaparkowanych pojazdów, ręczna rejestracja wjazdu i wyjazdu (przy awarii kamery), zamknięcie sesji bez opłaty (reklamacja/interwencja), wyszukiwanie po numerze rejestracyjnym.
- **Naliczanie opłat parkingowych** – aktywna taryfa, darmowy okres parkowania, stawka godzinowa zaokrąglana w górę, dzienny limit kwotowy.
- **Identity** z encjami `AppUser` (implementuje wspólny interfejs `ISystemUser`) i `AppRole` – trzy role: **Administrator**, **ParkingEmployee**, **Driver**.
- **JWT z access tokenem i refresh tokenem** – logowanie, odświeżanie tokenu, wylogowanie (revoke), profil zalogowanego użytkownika (`/me`); polityki haseł, blokada konta po nieudanych próbach.
- **Polityki autoryzacji** (`AppPolicies.AdminOnly`, `ParkingEmployeeOnly`, `ActiveUser`) – wymuszane atrybutami `[Authorize(Policy = …)]` na kontrolerach i konkretnych akcjach.
- **Reguła autorstwa zasobów** – sesje parkingowe i zdjęcia z kamer mogą być modyfikowane lub usuwane wyłącznie przez ich autora lub administratora (pole `CreatedById` + walidacja w serwisach z dedykowanym `ForbiddenAccessException` → HTTP 403).
- **Walidacja danych wejściowych** – FluentValidation z auto-rejestracją: `ParkingGateValidator`, `UpdateGateValidator`, `CreateTariffValidator`, `CameraCaptureValidator`.
- **Globalna obsługa wyjątków** – `ProblemDetailsExceptionHandler` mapuje wyjątki domenowe na ProblemDetails ze statusami HTTP 404 / 403 / 400, bez konieczności bloków `try/catch` w kontrolerach.
- **Seedowanie danych** – dwa idempotentne seedery uruchamiane przy starcie aplikacji w trybie Development: `IdentityDbSeeder` (role i 7 użytkowników – 2 administratorów, 3 pracowników, 2 kierowców) oraz `ParkingDataSeeder` (4 bramki, 3 taryfy, 12 pojazdów, 6 sesji – aktywne i zamknięte, 8 zdjęć z kamer). Dane w pełni polskie, z deterministycznymi GUID-ami.
- **Mapowanie encja ↔ DTO** – operatory implicit/explicit oraz dedykowane konstruktory rekordów, aby kontrolery nigdy nie zwracały surowych encji domenowych.
- **Testy jednostkowe (xUnit)** – pokrycie generycznego repozytorium, serwisu `ParkingGateService` (CRUD bramek, captures, autoryzacja) oraz `ParkingEmployeeService` (manualne wjazdy/wyjazdy, naliczanie opłaty w trzech scenariuszach: poniżej darmowego czasu, powyżej, dzienny limit, autorstwo).
- **Testy integracyjne** (`WebApplicationFactory` + SQLite in-memory) – pełen flow autoryzacji (`AuthApiTest`), CRUD bramek z politykami i autorstwem captures (`GatesApiTest`) oraz scenariusze Zadania 23 z weryfikacją 403 dla cudzej sesji i admin override (`EmployeeApiTest`).

## Stos technologiczny

| Komponent | Wersja |
|-----------|--------|
| .NET | 9.0 |
| Entity Framework Core | 9.0 |
| SQLite | – |
| ASP.NET Identity | 9.0 |
| JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`) | 9.0 |
| FluentValidation | – |
| xUnit | 2.9 |

## Struktura projektu

```
ASP/
├─ CoreApp/                    # warstwa domeny: encje, DTO, serwisy, interfejsy repozytoriów
│  ├─ Authorization/           # AppPolicies
│  ├─ Dto/
│  ├─ Entities/
│  ├─ Enums/
│  ├─ Exceptions/
│  ├─ Repositories/            # interfejsy + PagedResult
│  ├─ Services/                # IParkingGateService, IParkingEmployeeService, IAuthService, IDataSeeder
│  ├─ Users/                   # ISystemUser
│  └─ Validators/              # FluentValidation
├─ Infrastructure/             # warstwa danych (EF + Memory)
│  ├─ EntityFramework/
│  │  ├─ Context/              # ParkingDbContext
│  │  ├─ Entities/             # AppUser, AppRole
│  │  ├─ Repositories/         # EfGenericRepository i implementacje
│  │  └─ UnitOfWork/
│  ├─ Memory/                  # implementacje in-memory (testy jednostkowe)
│  ├─ Migrations/
│  ├─ Security/                # JwtSettings, AuthService, IdentityDbSeeder, ParkingDataSeeder, RefreshToken
│  ├─ Services/                # MemoryParkingGateService
│  └─ ParkingInfrastructureModule.cs
├─ WebApi/                     # warstwa prezentacji
│  ├─ Controllers/             # Auth, Gates, ParkingEmployee
│  ├─ Exceptions/              # ProblemDetailsExceptionHandler
│  └─ Program.cs
└─ UnitTest/
   ├─ Integration/             # AuthApiTest, GatesApiTest, EmployeeApiTest, ParkingAppTestFactory
   ├─ Services/                # ParkingEmployeeServiceTest, ParkingGateServiceTest
   └─ MemoryGenericRepositoryTest.cs

```

## Uruchomienie projektu

### Wymagania
- .NET SDK 9.0
- Narzędzie EF Core CLI

### Krok po kroku

```bash
# 1. Sklonuj repozytorium
git clone <link-do-repozytorium>
cd ASP

# 2. Przywróć narzędzia (m.in. dotnet-ef)
dotnet tool restore

# 3. Przywróć zależności i zbuduj
dotnet restore
dotnet build

# 4. Uruchom WebApi
dotnet run --project WebApi
```

Aplikacja domyślnie startuje pod `http://localhost:5084`. Profil `https` udostępnia również `https://localhost:7120`.

### Pierwsze uruchomienie

Przy uruchomieniu w trybie `Development`:
1. Tworzona jest baza `parking.db` (SQLite).
2. Wykonywane są oba seedery (Identity + dane domenowe).
3. Aplikacja jest gotowa do przyjmowania żądań.

## Konta testowe

| Rola | Email | Hasło |
|------|-------|-------|
| Administrator | `admin@parking.pl` | `Admin@123!` |
| Administrator | `anna.administrator@parking.pl` | `Admin@123!` |
| Pracownik | `jan.kowalski@parking.pl` | `Employee@123!` |
| Pracownik | `anna.nowak@parking.pl` | `Employee@123!` |
| Pracownik | `maria.wojcik@parking.pl` | `Employee@123!` |
| Kierowca | `piotr.kierowca@parking.pl` | `Driver@123!` |
| Kierowca | `marta.kierowca@parking.pl` | `Driver@123!` |

## Endpointy

### `/api/auth`
| Metoda | Ścieżka | Opis | Autoryzacja |
|--------|---------|------|-------------|
| POST | `/login` | Logowanie, zwraca access + refresh token | brak |
| POST | `/refresh` | Odświeżenie access tokenu | brak |
| POST | `/revoke` | Wylogowanie (unieważnienie refresh tokenu) | dowolny zalogowany |
| GET | `/me` | Profil zalogowanego użytkownika | dowolny zalogowany |

### `/api/gates`
| Metoda | Ścieżka | Opis | Autoryzacja |
|--------|---------|------|-------------|
| GET | `/` (paged) | Lista bramek | Administrator |
| GET | `/{id}` | Bramka po ID | dowolny zalogowany |
| POST | `/` | Utworzenie bramki | Administrator |
| PUT | `/{id}` | Aktualizacja bramki | Administrator |
| PATCH | `/{id}/status?isOperational=…` | Zmiana statusu | Administrator |
| POST | `/{gateId}/captures` | Dodanie zdjęcia z kamery | ParkingEmployee / Admin |
| GET | `/{gateId}/captures` | Lista zdjęć dla bramki | dowolny zalogowany |
| DELETE | `/{gateId}/captures/{captureId}` | Usunięcie zdjęcia | autor lub Admin |

### `/api/employee` 
| Metoda | Ścieżka | Opis | Autoryzacja |
|--------|---------|------|-------------|
| GET | `/sessions/active` | Aktywne sesje | ParkingEmployee / Admin |
| POST | `/sessions/entry` | Ręczna rejestracja wjazdu | ParkingEmployee / Admin |
| POST | `/sessions/{id}/exit` | Ręczna rejestracja wyjazdu (z opłatą) | autor lub Admin |
| POST | `/sessions/{id}/close-free` | Zamknięcie sesji bez opłaty | autor lub Admin |
| GET | `/sessions/search?plate=…` | Wyszukiwanie po numerze rejestracyjnym | ParkingEmployee / Admin |

## Plik `WebApi.http`

W katalogu [`WebApi/`](./WebApi) znajduje się plik [`WebApi.http`](./WebApi/WebApi.http) dla wbudowanego klienta HTTP, za porednictwem którego można przetestowac endpointy.

## Testy

```bash
dotnet test
```

Testy integracyjne podmieniają `ParkingDbContext` na in-memory SQLite (otwarte połączenie utrzymywane przez fixture), więc seedery wykonują się w pełni i można logować się na seedowanych użytkowników.

## Konfiguracja

Plik [`WebApi/appsettings.json`](./WebApi/appsettings.json):

```json
{
  "ConnectionStrings": {
    "ParkingDb": "Data Source=parking.db"
  },
  "Jwt": {
    "Issuer": "ParkingApi",
    "Audience": "ParkingClient",
    "SecretKey": "...",
    "ExpiryInMinutes": 60,
    "RefreshTokenDays": 7
  }
}
```