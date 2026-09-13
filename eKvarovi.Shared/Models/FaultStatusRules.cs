namespace eKvarovi.Shared.Models;

/// <summary>
/// Dopušteni prijelazi statusa prijave. Tablica stoji u zajedničkom projektu
/// da poslužitelj i sučelje rade po istim pravilima - sučelje po njoj nudi
/// izbor, a poslužitelj po njoj odbija nedopušteni zahtjev.
///
/// Redovni tijek:
/// Zaprimljeno -> Pregledano -> Dodijeljeno -> U radu -> Riješeno -> Zatvoreno
/// </summary>
public static class FaultStatusRules
{
    private static readonly Dictionary<int, int[]> Transitions = new()
    {
        // upravitelj pregledava prijavu i određuje vrstu, prioritet i rok
        [FaultStatusIds.Zaprimljeno] = [FaultStatusIds.Pregledano],

        // pregledana prijava čeka dodjelu izvršitelju
        [FaultStatusIds.Pregledano] = [FaultStatusIds.Dodijeljeno],

        // nalog se može i skinuti s izvršitelja pa se prijava vraća u red
        [FaultStatusIds.Dodijeljeno] = [FaultStatusIds.URadu, FaultStatusIds.Pregledano],

        // rad je u tijeku; uspješna intervencija podiže u Riješeno,
        // a skidanje naloga vraća prijavu u red čekanja
        [FaultStatusIds.URadu] = [FaultStatusIds.Rijeseno, FaultStatusIds.Pregledano],

        // upravitelj potvrđuje rezultat ili vraća prijavu na doradu
        [FaultStatusIds.Rijeseno] = [FaultStatusIds.Zatvoreno, FaultStatusIds.URadu],

        // zatvorena prijava se može ponovno otvoriti ako se kvar vrati
        [FaultStatusIds.Zatvoreno] = [FaultStatusIds.Pregledano]
    };

    public static IReadOnlyList<int> AllowedFrom(int currentStatusId) =>
        Transitions.TryGetValue(currentStatusId, out var allowed) ? allowed : [];

    public static bool IsAllowed(int fromStatusId, int toStatusId) =>
        AllowedFrom(fromStatusId).Contains(toStatusId);

    /// <summary>Zatvaranje traži obrazloženje završne provjere.</summary>
    public static bool RequiresClosingNote(int statusId) =>
        statusId == FaultStatusIds.Zatvoreno;

    /// <summary>Prijava je otvorena dok nije zatvorena.</summary>
    public static bool IsOpen(int statusId) =>
        statusId != FaultStatusIds.Zatvoreno;

    /// <summary>Na zatvorenu prijavu ne smiju se unositi nalozi ni intervencije.</summary>
    public static bool AcceptsWork(int statusId) =>
        statusId is not FaultStatusIds.Zatvoreno;
}
