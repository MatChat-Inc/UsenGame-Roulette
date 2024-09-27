// Created by LunarEclipse on 2024-6-3 9:20.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

namespace USEN.Games.Roulette
{
    [Table("roulettes")]
    public class RouletteData
    {
        [PrimaryKey] [AutoIncrement] [JsonIgnore] 
        public int ID { get; set; }

        public string Title { get; set; }
        public long Timestamp { get; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public string SectorJson
        {
            get => JsonConvert.SerializeObject(sectors);
            set => sectors = JsonConvert.DeserializeObject<List<RouletteSector>>(value);
        }
        
        public List<RouletteSector> sectors = new();
        

        public string category;

        public RouletteData()
        {
            // ID = Guid.NewGuid().ToString();
        }
        
        // Copy constructor.
        public RouletteData(RouletteData other)
        {
            ID = other.ID;
            Title = other.Title;
            sectors = new();
            for (int i = 0; i < other.sectors.Count; i++)
                sectors.Add(new RouletteSector(other.sectors[i]));
        }

        public void OnValidate()
        {
            
            for (int i = 0; i < sectors.Count; i++)
            {
                var sector = sectors[i];
                sector.id = sectors.IndexOf(sector);
                sector.color = Color.HSVToRGB(1.0f / sectors.Count * i, 0.5f, 1f);
            }
        }
    }
}