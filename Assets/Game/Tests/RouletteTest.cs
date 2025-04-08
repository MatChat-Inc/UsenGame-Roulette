using UnityEngine;
using USEN.Games.Roulette;

public class RouletteTest : MonoBehaviour
{
    public RouletteWheel rouletteWheel;
    
    void Start()
    {
        rouletteWheel.RouletteData = RouletteManager.Instance.GetRandomRoulette();
    }

    void Update()
    {
        
    }
}
