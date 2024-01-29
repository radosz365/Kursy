using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

app.UseStaticFiles();

app.MapGet("/", () => "Witaj w API Kursów!");

app.MapGet("/kursy", async () => await ReadJsonFile<Kurs>("kursy.json"));
app.MapPost("/kursy", async (Kurs kurs) => await SaveToJsonFile("kursy.json", kurs));
app.MapGet("/uczestnicy", async () => await ReadJsonFile<Uczestnik>("uczestnicy.json"));
app.MapPost("/uczestnicy", async (Uczestnik uczestnik) => await SaveToJsonFileWithId("uczestnicy.json", uczestnik, currentData => currentData.Max(u => u.Id)));

app.Run();

async Task<List<T>> ReadJsonFile<T>(string filePath) where T : new()
{
    if (!File.Exists(filePath))
    {
        return new List<T>();
    }

    var json = await File.ReadAllTextAsync(filePath);
    return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
}

async Task SaveToJsonFile<T>(string filePath, T data)
{
    var currentData = await ReadJsonFile<T>(filePath);
    currentData.Add(data);
    var json = JsonSerializer.Serialize(currentData, new JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(filePath, json);
}

async Task SaveToJsonFileWithId<T>(string filePath, T data, Func<List<T>, int> getId)
{
    var currentData = await ReadJsonFile<T>(filePath);
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
