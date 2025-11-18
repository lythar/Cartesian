using System.Text.Json.Serialization;

namespace Cartesian.Services.Events;


[JsonConverter(typeof(JsonStringEnumConverter<EventTag>))]
public enum EventTag
{
    Outdoor,
    Sport,
    Fitness,
    Literature,
    Business,
    Tech,
    Educational,
    Kids,
    Family,
    Parenting,
    Conference,
    Film,
    Fashion,
    Running,
    Cycling,
    BoardGames,
    VideoGames,
    Entertainment,
    Comedy,
    Arts,
    Hobby,
    Party,
    Gathering,
    Charity,
    Volunteering,
    Environmental,
    Festival,
    Concert,
    Food,
    Travel,
    Religious,
    Study,
    Market,
    Political
}
