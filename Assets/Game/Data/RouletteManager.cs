// Created by LunarEclipse on 2024-7-13 19:34.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

namespace USEN.Games.Roulette
{
    public class RouletteManager
    {
        public static RouletteManager Instance { get; } = new();
        
        public readonly SQLiteConnection db;

        private long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        private RouletteManager(string databaseName = null)
        {
            var databasePath = Path.Combine(Application.persistentDataPath, "DB", databaseName ?? "roulette.db");
            var databaseDirectory = Path.GetDirectoryName(databasePath);

            // Create the directory if it doesn't exist.
            if (!Directory.Exists(databaseDirectory))
                Directory.CreateDirectory(databaseDirectory!);

            db = new SQLiteConnection(databasePath);
            db.CreateTable<RouletteData>();
            
            var all = db.Table<RouletteData>().ToList();

            if (!db.Table<RouletteData>().Any())
            {
                Debug.Log("[RouletteManager] Database is empty. Inserting default data.");
                
                var json = Resources.Load<TextAsset>("roulette").text;
                var categories = JsonConvert.DeserializeObject<RouletteCategories>(json);
                db.RunInTransaction(() => {
                    foreach (var category in categories.categories)
                        foreach (var roulette in category.roulettes)
                        {
                            roulette.category = category.title;
                            db.Insert(roulette);
                        }
                });
            }

        }
        
        public Task<RouletteCategories> Sync()
        {
            return Usen.API.GetRoulettes().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task.Exception);
                    return null;
                }

                var categories = task.Result.categories;
                db.RunInTransaction(() =>
                {
                    db.DeleteAll<RouletteData>();
                    foreach (var category in categories)
                        foreach (var roulette in category.roulettes)
                        {
                            roulette.category = category.title;
                            db.Insert(roulette);
                        }
                });


                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                
                return task.Result;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public List<RouletteCategory> GetCategories()
        {
            var data = db.Table<RouletteData>().ToList();
            var categories =
                from roulette in data
                group roulette by roulette.category
                into g
                select new RouletteCategory
                {
                    title = g.Key,
                    roulettes = g.ToList()
                };
            
            return categories.ToList();
        }
        
        public RouletteCategory GetCategory(string title)
        {
            var data = db.Table<RouletteData>().ToList();
            var roulettes =
                from roulette in data
                where roulette.category == title
                select roulette;
            
            return new RouletteCategory
            {
                title = title,
                roulettes = roulettes.ToList()
            };
        }

        public RouletteData GetRandomRoulette()
        {
            var data = db.Table<RouletteData>().ToList();
            
            if (data.Count == 0) return null;

            var result =
                from roulette in data
                where roulette.category == "バツゲーム"
                select roulette;
            var batuGames = result.ToList();
            
            return batuGames[UnityEngine.Random.Range(0, batuGames.Count - 1)];
        }
        
        public void AddRoulette(RouletteData roulette)
        {
            db.Insert(roulette);
        }
        
        public void InsertFromJsonList(string json)
        {
            var questions = JsonConvert.DeserializeObject<List<RouletteData>>(json);

            // New transaction to add all questions at once.
            db.RunInTransaction(() =>
            {
                foreach (var question in questions)
                    db.Insert(question);
            });
        }
    }
}