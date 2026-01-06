using System.ComponentModel.DataAnnotations;

namespace MvcMovie.Models;

public class Movie
{
    public string id {get;set;}
    public string? Title {get;set;}
    [DataType(DataType.Date)]
    public DateTime? ReleaseDate {get;set;}
    public string? Genre {get;set;}
    public decimal? Price {get;set;}

    public string partition1 {get;set;}

    public Movie()
    {
        id = Guid.NewGuid().ToString();
        partition1 = MvcMovie.Static.Static.PartitionKey;

    }
}