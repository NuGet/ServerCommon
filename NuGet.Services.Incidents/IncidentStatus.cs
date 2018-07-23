namespace NuGet.Services.Incidents
{
    public enum IncidentStatus
    {
        Holding,
        Active,
        Mitigated,
        Resolved,
        Suppressed,
        New,
        Correlating,
        Mitigating
    }
}
