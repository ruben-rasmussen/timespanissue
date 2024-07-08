using Marten;
using Microsoft.AspNetCore.Mvc;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMarten(options =>
{
    options.Connection("CONNECTIONSTRING");
    options.UseSystemTextJsonForSerialization();
    options.AutoCreateSchemaObjects = AutoCreate.All;
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/create", async ([FromServices] IDocumentSession session) =>
{
    session.Insert(new DataWithTimeSpan { Id = Guid.NewGuid(), Working = true, TimeSpan = new TimeSpan(0, 3, 4, 5, 1) });
    session.Insert(new DataWithTimeSpan { Id = Guid.NewGuid(), Working = false, TimeSpan = new TimeSpan(1, 2, 3, 4, 5) });

    await session.SaveChangesAsync();
});

app.MapGet("/works", ([FromServices] IQuerySession session) => session.Query<DataWithTimeSpan>().ToList());

app.MapGet("/works1", ([FromServices] IQuerySession session) => session.Query<DataWithTimeSpan>().Where(t => t.Working).Select(t => new MySpecialSelect { TimeSpan = t.TimeSpan }).ToList());

//this fails due to 1. when day > 1
app.MapGet("/fails", ([FromServices] IQuerySession session) => session.Query<DataWithTimeSpan>().Select(t => t.TimeSpan).ToList());

//this fails due to 1. when day > 1
app.MapGet("/fails2", ([FromServices] IQuerySession session) => session.Query<DataWithTimeSpan>().Select(t => new MySpecialSelect { TimeSpan = t.TimeSpan }).ToList());

// this fails with leading zero before 3..
app.MapGet("/fails3", ([FromServices] IQuerySession session) => session.Query<DataWithTimeSpan>().Where(t => t.Working).Select(t => t.TimeSpan).ToList());
app.Run();


public class MySpecialSelect
{
    public TimeSpan TimeSpan { get; set; }
}
public class DataWithTimeSpan
{
    public Guid Id { get; set; }
    public bool Working { get; set; } = true;
    public TimeSpan TimeSpan { get; set; }
}