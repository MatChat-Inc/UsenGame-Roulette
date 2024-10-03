// Created by LunarEclipse on 2024-6-21 1:53.

using System.Linq;
using System.Threading.Tasks;
using Luna;
using Luna.UI;
using Luna.UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace USEN.Games.Roulette
{
    public class RouletteStartView : Widget
    {
        public Button startButton;
        public Button settingsButton;
        
        public AudioClip bgmClip;
        
        private RouletteManager _manager;
        private RouletteCategories _categories;
        
        private void Start()
        {
            BgmManager.Play(bgmClip);
            
            // Preload all roulette widgets
            Widget.Load(GetType().Namespace);
            
            // Load the roulette data
            RouletteManager.Instance.Sync().ContinueWith(async task => {
                _categories = task.Result;
                EventSystem.current.SetSelectedGameObject(startButton.gameObject);
                startButton.interactable = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
            
            Navigator.Instance.onPopped += (route) => {
                SFXManager.Play(R.Audios.SfxRouletteBack);
            };
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) {
                OnExitButtonClicked();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                BgmManager.Play(bgmClip);
            }
        }

        private void OnDestroy()
        {
            BgmManager.Stop();
            
            // Unload all roulette widgets
            // Widget.Unload(GetType().Namespace);
        }

        public void OnStartButtonClicked()
        {
            Navigator.Push<RouletteCategoryView>((view) => {
                view.Categories = _categories.categories;
            });
        }

        public void PlayRandomGame()
        {
            var category = _categories.categories.First(); //[Random.Range(0, _dataset.categories.Count)];
            var rouletteData = category.roulettes[Random.Range(0, category.roulettes.Count)];
            Navigator.Push<RouletteGameView>((view) => {
                view.RouletteData = rouletteData;
            });
        }

        public void OnSettingsButtonClicked()
        {
            Navigator.Push<RouletteSettingsView>();
        }
        
        public void OnExitButtonClicked()
        {
            SceneManager.LoadScene("GameEntries");
        }
    }
}