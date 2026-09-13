# e-Kvarovi Županije — plan testiranja

Prođi redom. Uz svaku stavku piše **što točno očekivati** — ako se ne poklapa, to je greška.

---

## 0. Priprema

Testiranje je mijenjalo demo podatke, pa krećemo od čiste baze.

```powershell
cd C:\Users\katar\Desktop\eKvarovi
Stop-Process -Name eKvarovi.Api,eKvarovi.App -Force -ErrorAction SilentlyContinue
Remove-Item eKvarovi.Api\eKvarovi.db* -Force -ErrorAction SilentlyContinue
dotnet build
```

Build mora proći bez grešaka. Zatim **dva prozora**:

```powershell
# prozor 1
dotnet run --project eKvarovi.Api
```

```powershell
# prozor 2
dotnet run --project eKvarovi.App
```

Otvori `http://localhost:5066` i pritisni **Ctrl+F5**.

### Polazno stanje nakon seeda

| # | Prijava | Status | Lokacija | Izvršitelj |
|---|---|---|---|---|
| 1 | Ne radi rasvjeta u učionici 12 | Zaprimljeno | OŠ Preradovića | — |
| 2 | Puca vodovodna cijev u podrumu | Pregledano | Dom zdravlja | — |
| 3 | Curi slavina u sanitarnom čvoru | Dodijeljeno | Dom zdravlja | Babić |
| 4 | Kotlovnica se gasi tijekom noći | U radu | SŠ Daruvar | Babić (2. nalog) |
| 5 | Projektor u dvorani | Riješeno | Vrtić Sunce | Novak |
| 6 | Ulazna vrata se ne zaključavaju | Zatvoreno | Zgrada uprave | Horvat |
| 7 | Iskri utičnica u zbornici | Dodijeljeno | OŠ Preradovića | Horvat |

Računi:

| E-mail | Lozinka | Uloga |
|---|---|---|
| `admin@ekvarovi.local` | `Admin123!` | Administrator + Upravitelj |
| `voditelj@ekvarovi.local` | `Voditelj123!` | Upravitelj |
| `serviser@ekvarovi.local` | `Serviser123!` | Izvršitelj (Ivan Horvat) |
| `serviser2@ekvarovi.local` | `Serviser123!` | Izvršitelj (Marko Babić) |
| `prijavitelj@ekvarovi.local` | `Prijava123!` | Prijavitelj (Ana Kovačević) |

---

## 1. Prijava u sustav

- [ ] Otvori se stranica za prijavu s gradijentnim zaglavljem
- [ ] Upiši krivu lozinku → crvena poruka **"Neispravan e-mail ili lozinka."**
- [ ] Klikni demo gumb **Prijavitelj** → polja se popune sama
- [ ] Prijavi se → otvara se nadzorna ploča
- [ ] **Osvježi stranicu (F5)** → i dalje si prijavljena, ne traži ponovnu prijavu

> Ovo zadnje dokazuje da prijava preživljava osvježavanje jer token stoji u zaštićenoj pohrani preglednika.

---

## 2. Prijavitelj — Ana Kovačević

`prijavitelj@ekvarovi.local` / `Prijava123!`

### Izbornik
- [ ] Gore desno piše **Ana Kovačević** i ispod **Prijavitelj**
- [ ] U izborniku postoje: Nadzorna ploča, Prijavi kvar, Moje prijave
- [ ] U izborniku **NEMA**: Sve prijave, Radni nalozi, Intervencije, Lokacije, Djelatnici, Korisnički računi

### Nadzorna ploča
- [ ] Kartica **Moje prijave** pokazuje **3** (prijave 1, 6 i 7)

### Moje prijave
- [ ] Vidi **3 kartice**, ne svih 7
- [ ] Kartica "Ne radi rasvjeta" ima oznaku **"čeka pregled"** (nema još prioritet)
- [ ] Kartica "Ulazna vrata" je siva, status **Zatvoreno**, ispod nje piše obrazloženje

### Unos nove prijave
- [ ] Klikni **Prijavi kvar**
- [ ] Polje **Lokacija** je **zaključano** i piše *"Kvar možete prijaviti samo na svojoj matičnoj lokaciji"* → mora stajati **Osnovna škola Petra Preradovića**
- [ ] Nema polja za vrstu kvara, prioritet ni rok — to postavlja upravitelj
- [ ] Ostavi naslov prazan → **Spremi** ne prolazi, javi da je naslov obavezan
- [ ] Upiši naslov *"Ne radi projektor u učionici 5"* i opis → **Prijavi kvar**
- [ ] Otvara se stranica s detaljima, status **Zaprimljeno**

### Privitak
- [ ] Na detaljima prijave nađi **Privitci uz prijavu**
- [ ] Namjena je **Fotografija prije rada** → klikni **Priloži**, odaberi bilo koju sliku
- [ ] Slika se pojavi kao sličica, ispod nje veličina i datum
- [ ] Prebaci namjenu na **Dokument** → priloži **PDF** → prikaže se ikona PDF-a s poveznicom
- [ ] Vrati namjenu na **Fotografija prije rada** i pokušaj priložiti PDF → **odbija** uz poruku da su dopuštene samo slike
- [ ] Klikni koš na slici → potvrdi → slika nestaje

### Ograničenja
- [ ] Na detaljima prijave **nema** polja za pregled, dodjelu ni promjenu statusa
- [ ] U adresu upiši `localhost:5066/kvarovi` → stranica se **ne smije** otvoriti s podacima

---

## 3. Upravitelj — Tomislav Jurić

`voditelj@ekvarovi.local` / `Voditelj123!`

### Popis svih prijava
- [ ] **Sve prijave** → vidi **8** (7 iz seeda + tvoja nova)
- [ ] Upiši u pretragu *"kotlovnica"* → ostaje **1** redak
- [ ] Obriši pretragu, odaberi **Vrsta = Voda** → **2** retka
- [ ] Označi **Probijen rok** → ostaje **Kotlovnica** (rok joj je u prošlosti, ćelija roka je crvena)
- [ ] Označi **Bez izvršitelja** → prijave koje čekaju dodjelu
- [ ] Klikni ikonu **poništi filtere** → vraća se svih 8
- [ ] Klikni zaglavlje **Prioritet** pa **Rok** → redoslijed se mijenja

> Filteri se šalju API-ju kao query parametri. Da je filtriranje u pregledniku, brojka "od ukupno" ne bi se mijenjala.

### Pregled prijave
- [ ] Otvori **"Ne radi rasvjeta u učionici 12"** (status Zaprimljeno)
- [ ] Desno postoji kartica **Pregled prijave**
- [ ] Odaberi **Prioritet = Kritičan**, ostavi rok prazan → **odbija**: *"Prijava kritičnog prioriteta mora imati određen rok."*
- [ ] Postavi **Vrsta = Elektrika**, **Prioritet = Visok**, rok za 5 dana → **Označi pregledanim**
- [ ] Status prelazi u **Pregledano**, pojavljuje se kartica **Dodjela izvršitelju**

### Dodjela
- [ ] Odaberi **Ivan Horvat**, upiši uputu → **Dodijeli**
- [ ] Status prelazi u **Dodijeljeno**, desno se pojavi ime izvršitelja
- [ ] Na vremenskoj crti desno pojavi se prvi nalog

### Preraspodjela — ključni test
- [ ] Na istoj prijavi odaberi **Marko Babić**, upiši **Razlog preraspodjele**
- [ ] **Preraspodijeli**
- [ ] Na vremenskoj crti su sada **dva naloga**:
  - Babić — oznaka **Aktivan**
  - Horvat — oznaka **Zatvoren**, s datumom zatvaranja i upisanim razlogom
- [ ] **Prethodni nalog nije nestao** — to je zahtjev specifikacije

### Zabrane
- [ ] Otvori **"Kotlovnica"** (U radu) → padajući izbornik statusa **ne nudi** Zatvoreno
- [ ] Otvori **"Projektor"** (Riješeno) → nudi Zatvoreno; odaberi ga, ostavi obrazloženje prazno → **odbija**
- [ ] Upiši obrazloženje → **Promijeni status** → prijava je **Zatvoreno**

### Ostali popisi
- [ ] **Radni nalozi** → vidi sve naloge, filter po izvršitelju radi
- [ ] **Intervencije** → filtriraj po statusu **Završena** i po izvršitelju
- [ ] **Lokacije** → pokušaj obrisati **Dom zdravlja** → odbija jer ima prijavljene kvarove
- [ ] **Lokacije** → dodaj novu, odaberi vrstu iz šifarnika
- [ ] U izborniku **nema** Korisnički računi

---

## 4. Izvršitelj — Ivan Horvat

`serviser@ekvarovi.local` / `Serviser123!`

### Moji radni nalozi
- [ ] Vidi nalog **"Iskri utičnica u zbornici"** (i onaj koji ti je upravitelj dodijelio, ako ga nisi preraspodijelila)
- [ ] Kartica ima crvenu traku uz lijevi rub — prioritet je Kritičan
- [ ] **NE vidi** naloge Marka Babića

### Rad na intervenciji
- [ ] Klikni **Pokreni intervenciju**
- [ ] Otvara se stranica intervencije, status **U tijeku**
- [ ] Vrati se na detalje kvara → status prijave je sada **U radu**
- [ ] Na intervenciji upiši bilješku → **Spremi bilješku**

### Materijal
- [ ] Odaberi materijal **Žarulja LED 10W** → cijena se popuni sama iz šifarnika
- [ ] Količina **3** → **Dodaj materijal** → u tablici iznos **12,60 €**
- [ ] Dodaj **isti** materijal ponovno, količina 2 → redak se **ne udvostručuje**, količina postaje **5**
- [ ] Upiši količinu **0** → odbija

### Fotografija nakon rada
- [ ] U dijelu **Fotografije i dokumenti** namjena je **Fotografija nakon rada**
- [ ] Priloži sliku → pojavi se sličica

### Neuspješan pokušaj
- [ ] Obriši bilješku i klikni **Završi kao uspješnu** → **odbija**, bilješka je obavezna
- [ ] Vrati bilješku → klikni **Završi kao neuspješnu**
- [ ] Žuta poruka: intervencija je neuspješna, može se otvoriti nova
- [ ] Vrati se na nalog → **Pokreni intervenciju** ponovno radi
- [ ] Prva intervencija **i dalje stoji** u povijesti na detaljima kvara

### Uspješan završetak — automatsko Riješeno
- [ ] Na novoj intervenciji upiši bilješku → **Završi kao uspješnu**
- [ ] Zelena poruka o uspjehu
- [ ] Otvori detalje kvara → status je **Riješeno**, bez ikakve radnje upravitelja
- [ ] Polje **Riješeno** ima upisan datum i vrijeme
- [ ] Trajanje intervencije je izračunato iz početka i završetka

### Zabrane
- [ ] Na detaljima kvara **nema** kartice za pregled, dodjelu ni promjenu statusa
- [ ] U adresu upiši `localhost:5066/korisnici` → ne otvara se

---

## 5. Upravitelj zatvara

`voditelj@ekvarovi.local`

- [ ] Otvori kvar koji je Horvat upravo riješio → status **Riješeno**
- [ ] Status → **Zatvoreno**, upiši obrazloženje završne provjere
- [ ] Prijava je **Zatvoreno**, obrazloženje se vidi na kartici
- [ ] Vremenska crta i dalje prikazuje **sve** naloge i **sve** intervencije, uključujući neuspješnu

---

## 6. Administrator

`admin@ekvarovi.local` / `Admin123!`

### Korisnički računi
- [ ] Vidi 5 računa s ulogama u boji
- [ ] **Novi račun** → e-mail, lozinka, poveži s djelatnikom **Petra Novak**, uloga **Izvršitelj** → Spremi
- [ ] Odjavi se, prijavi se novim računom → vidi **Moji radni nalozi**
- [ ] Vrati se kao admin → uredi vlastiti račun, makni ulogu **Administrator** → **odbija**
- [ ] Uredi vlastiti račun, isključi **Aktivan** → **odbija**

### Šifarnici
- [ ] **Djelatnici** → dodaj djelatnika, označi **Izvršitelj**
- [ ] **Materijali** → dodaj materijal s mjernom jedinicom i cijenom
- [ ] Pokušaj obrisati **Žarulja LED 10W** (ako si je trošila) → odbija jer je utrošena

### Prijava u tuđe ime
- [ ] **Prijavi kvar** → admin nema vezanog djelatnika, pa polje **Prijavitelj** piše *(obavezno)*
- [ ] Ostavi prazno → odbija
- [ ] Odaberi prijavitelja i lokaciju → prolazi

---

## 7. Nadzorna ploča

Prijavi se kao **voditelj**:

- [ ] **Otvoreni kvarovi**, **Probijen rok**, **Bez izvršitelja**, **Kritični otvoreni**
- [ ] **Aktivne intervencije** — one koje su pokrenute a nisu zaključene
- [ ] **Čeka potvrdu** — riješene, još nezatvorene
- [ ] **Prosječno rješavanje** u danima
- [ ] Grafovi **Po statusu**, **Po vrsti kvara**, **Najopterećenije lokacije**
- [ ] **Zadnjih pet prijava** — klik na naslov vodi na detalje

Kao **izvršitelj** dodatno:
- [ ] **Moji nalozi** i **Moje intervencije** s osobnim brojkama

---

## Ako nešto pukne

Zapiši:
1. Koja stranica i koja uloga
2. Točan tekst crvene poruke
3. Iz prozora gdje radi API — zadnjih par redaka ispisa

To je dovoljno da se greška nađe.
