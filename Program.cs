using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

var rewriteOptions = new RewriteOptions()
    .AddRedirect("^$", "index.html");
app.UseRewriter(rewriteOptions);

app.UseStaticFiles();

app.MapGet("/", () => "Witaj!");

app.MapGet("/kursy", async () => await ReadJsonFile("kursy.json", new List<Kurs>()));
app.MapPost("/kursy", async (Kurs kurs) => await SaveToJsonFile("kursy.json", kurs));
app.MapGet("/uczestnicy", async (int? kursId) => 
{
    var uczestnicy = await ReadJsonFile("uczestnicy.json", new List<Uczestnik>());
    if (kursId.HasValue)
    {
        uczestnicy = uczestnicy.Where(u => u.KursId == kursId.Value).ToList();
    }
    return uczestnicy;
});
app.MapPost("/uczestnicy", async (Uczestnik uczestnik) => await SaveToJsonFileWithId("uczestnicy.json", uczestnik, currentData => currentData.Max(u => u.Id)));

app.Run();

async Task<List<T>> ReadJsonFile<T>(string filePath, List<T> defaultValue)
{
    if (!File.Exists(filePath))
    {
        return defaultValue;
    }

    var json = await File.ReadAllTextAsync(filePath);
    return JsonSerializer.Deserialize<List<T>>(json) ?? defaultValue;
}

async Task SaveToJsonFile<T>(string filePath, T data)
{
    var currentData = await ReadJsonFile(filePath, new List<T>());
    currentData.Add(data);
    var json = JsonSerializer.Serialize(currentData, new JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(filePath, json);
}

async Task SaveToJsonFileWithId<T>(string filePath, T data, Func<List<T>, int> getId)
{
    var currentData = await ReadJsonFile(filePath, new List<T>());
    var newId = currentData.Any() ? getId(currentData) + 1 : 1;
    var idProperty = typeof(T).GetProperty("Id");
    if (idProperty != null)
    {
        idProperty.SetValue(data, newId);
    }
    currentData.Add(data);
    var json = JsonSerializer.Serialize(currentData, new JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(filePath, json);
}

class Kurs
{
    public int Id { get; set; }
    public string? Nazwa { get; set; }
    public string? Opis { get; set; }
}

class Uczestnik
{
    public int Id { get; set; }
    public int KursId { get; set; }
    public string? Imie { get; set; }
    public string? Nazwisko { get; set; }
    public string? Email { get; set; }
}

