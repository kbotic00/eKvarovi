# e-Kvarovi Županije

Aplikacija za prijavu i obradu kvarova na županijskim lokacijama — od prijave
djelatnika, preko dodjele izvršitelju i intervencije na terenu, do zatvaranja
uz završnu provjeru.

Izrađeno kao završni projekt. Svi podaci su demo podaci.

---

## Tehnologije

| Sloj | Tehnologija |
|---|---|
| API | ASP.NET Core 10, EF Core 10, SQLite |
| Sučelje | Blazor Web App (Interactive Server), MudBlazor 9 |
| Autentikacija | JWT Bearer, uloge, pravila pristupa |

Rješenje ima tri projekta:

```
eKvarovi.Api      REST API, baza, poslovna pravila
eKvarovi.App      Blazor sučelje
eKvarovi.Shared   modeli i DTO-ovi koje dijele oba
```

---

## Pokretanje

Potreban je .NET 10 SDK.

```bash
git clone <adresa-repozitorija>
cd eKvarovi
dotnet build
```

Zatim u **dva odvojena prozora**:

```bash
dotnet run --project eKvarovi.Api     # http://localhost:5203
```

```bash
dotnet run --project eKvarovi.App     # http://localhost:5066
```

Baza se stvara sama pri prvom pokretanju API-ja — migracija napuni šifarnike,
a seeder demo podatke. Nije potrebno ništa ručno pripremati.

Dokumentacija API-ja: `http://localhost:5203/swagger`

---

## Demo računi

| E-mail | Lozinka | Uloga |
|---|---|---|
| `admin@ekvarovi.local` | `Admin123!` | Administrator + Upravitelj |
| `voditelj@ekvarovi.local` | `Voditelj123!` | Upravitelj |
| `serviser@ekvarovi.local` | `Serviser123!` | Izvršitelj (Ivan Horvat) |
| `serviser2@ekvarovi.local` | `Serviser123!` | Izvršitelj (Marko Babić) |
| `prijavitelj@ekvarovi.local` | `Prijava123!` | Prijavitelj (Ana Kovačević) |

---

## Tok prijave

```
Zaprimljeno ──pregled──► Pregledano ──dodjela──► Dodijeljeno
      ▲                       ▲                       │
      └── skidanje naloga ────┘            pokretanje intervencije
                                                      ▼
Zatvoreno ◄──završna provjera── Riješeno ◄──uspješan završetak── U radu
```

Tri prijelaza događaju se sami:

- dodjela izvršitelju → **Dodijeljeno**
- pokretanje intervencije → **U radu**
- uspješno završena intervencija → **Riješeno**

U **Zatvoreno** prelazi samo upravitelj, i to tek nakon što postoji uspješno
završena intervencija.

---

## Uloge

| Uloga | Ovlasti |
|---|---|
| **Administrator** | Korisnički računi, svi šifarnici, puni pristup |
| **Upravitelj** | Pregled prijava, određivanje vrste/prioriteta/roka, dodjela i preraspodjela, zatvaranje |
| **Izvršitelj** | Vlastiti radni nalozi, intervencije, materijal, fotografije |
| **Prijavitelj** | Prijava kvara na vlastitoj lokaciji, praćenje svojih prijava |

Jedan račun može imati više uloga. Endpointi `/mine` čitaju identitet
isključivo iz JWT claima `employee_id`, nikad iz tijela zahtjeva.

---

## Model baze


Tri odluke koje oblikuju model:

**Povijest dodjela nosi zasebna tablica `WorkAssignments`**, a ne polje
`TechnicianId` na prijavi. Ponovna dodjela zatvara stari nalog i otvara novi;
prethodni ostaje netaknut. Filtrirani jedinstveni indeks
`WHERE IsActive = 1` jamči da po prijavi postoji najviše jedan aktivan nalog —
pravilo čuva baza, ne samo kod.

**Intervencije vise uz nalog, ne uz prijavu.** Neuspješna intervencija ostaje
zabilježena, a na istom nalogu se otvara nova bez gubitka prethodne.

**Vrsta kvara, prioritet i rok su `nullable`** jer ih prijavitelj ne zna niti
smije određivati — popunjava ih upravitelj pri pregledu. Prijava u statusu
`Zaprimljeno` legitimno postoji bez njih.

Pravila su zapisana kao podaci: `FaultPriority.RequiresDeadline` i
`InterventionStatus.IsSuccessful` čitaju se iz šifarnika umjesto da budu
zakucani uvjeti u kodu.

---

## Poslovna pravila koja API provodi

- prijava mora pripadati **aktivnoj** lokaciji
- prijavitelj prijavljuje kvar **samo na svojoj matičnoj lokaciji**
- prijava se dodjeljuje tek **nakon pregleda**
- prijava **kritičnog prioriteta mora imati rok**
- po prijavi postoji najviše **jedan aktivan** radni nalog
- izvršitelj radi **samo na svojem** aktivnom nalogu
- na jednom nalogu ne smiju teći **dvije nezaključene** intervencije
- završena intervencija mora imati **početak, kraj i bilješku**
- prijava se **ne može zatvoriti** bez uspješno završene intervencije
- zatvaranje traži **obrazloženje** završne provjere
- količina materijala mora biti **veća od nule**
- prijava s intervencijama **ne briše se fizički**
- anonimni pozivi vraćaju **401**, neovlašteni **403**

Skrivanje akcije u sučelju nigdje nije jedina zaštita — ista ovlast provjerava
se i na API endpointu.

---

## Privitci

Fotografije prije i nakon rada te PDF dokumenti. Provjerava se vrsta i veličina
datoteke, fizičko ime generira poslužitelj (GUID), a izvorno ime čuva se samo
za prikaz. Brisanje uklanja i zapis u bazi i datoteku s diska.


