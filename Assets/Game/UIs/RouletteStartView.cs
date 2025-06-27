// Created by LunarEclipse on 2024-6-21 1:53.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Luna;
using Luna.Extensions;
using Luna.UI;
using Luna.UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace USEN.Games.Roulette
{
    public class RouletteStartView : Widget
    {
        public Button startButton;
        public Button settingsButton;
        
        private RouletteManager _manager;
        private RouletteCategories _categories;
        
        private Task<RouletteCategories> _httpTask;
        
        private async void Start()
        {
            // Audio volume
            BgmManager.Volume = RoulettePreferences.BgmVolume;
            SFXManager.Volume = RoulettePreferences.SfxVolume;
            
            // Show loading indicator before necessary assets are loaded
            await UniTask.Yield(PlayerLoopTiming.PreLateUpdate);
            Navigator.ShowModal<RoundedCircularLoadingIndicator>();
            
            // Preload audios
            var bgmTask = R.Audios.BgmRouletteLoop.Load();
            bgmTask.Then(BgmManager.Play);
            var audioTask =  Assets.Load(GetType().Namespace, "Audio");
            
            await Task.WhenAll(bgmTask, audioTask, _httpTask);
            
            
            Navigator.PopToRoot();
            Navigator.Instance.onPopped += (route) => {
                SFXManager.Play(R.Audios.SfxBack);
            };
        }

        private void OnEnable()
        {
            // Load the roulette data
            _httpTask = RouletteManager.Instance.Sync();
            _httpTask.ContinueWith(async task => {
                _categories = task.Result;
                if (startButton.interactable == false)
                    EventSystem.current.SetSelectedGameObject(startButton.gameObject);
                startButton.interactable = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
            
            API.GetRandomSetting().ContinueWith(task => {
                RoulettePreferences.DisplayMode = (RouletteDisplayMode) task.Result.random;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) {
                OnExitButtonClicked();
            }
                        
#if UNITY_ANDROID
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AndroidPreferences.Toast("Hello, Kotlin!");
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Luna.Android.ShowToast("Hello, Android!");
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Luna.Android.ShowToast(USEN.AndroidPreferences.TVIdentifier);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Luna.Android.ShowToast(USEN.AndroidPreferences.Ssid);
            }
#endif
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
            Application.Quit();
// #if UNITY_ANDROID
//             Android.Back();
// #endif
        }
    }
}