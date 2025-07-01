using System;
using MongoDB.Driver;
using MongoDB.Entities;
using System.Text.Json;

namespace SearchService.Data;

public class DbInitializer
{

    public static async Task InitializeAsync(WebApplication app)
    {
        // Initialize the database connection
        await MongoDB.Entities.DB.InitAsync("SearchDb", MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        // Create indexes for the Item collection
        await MongoDB.Entities.DB.Index<Models.Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Models.Item>();
        if (count == 0)
        {
            System.Console.WriteLine("Seeding database with initial data...");
            // Seed the database with initial data if it's empty
            var itemData = await File.ReadAllTextAsync("Data/auctions.json");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            var items = JsonSerializer.Deserialize<List<Models.Item>>(itemData, options);

            await DB.SaveAsync(items);
        }
    }

}
