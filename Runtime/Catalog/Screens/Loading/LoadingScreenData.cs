using System;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Screens
{
    [Serializable]
    public class LoadingScreenData
    {
        [Tooltip("Top label. Empty = hidden.")]
        public string Title = "Loading...";
        [Tooltip("Secondary label for live status text. Empty = hidden.")]
        public string Subtitle = "";
        [Tooltip("Progress bar starts at this value on Bind. Clamped [0,1]. Snaps instantly (no tween).")]
        public float InitialProgress = 0f;
        [Tooltip("Show the progress bar Image. False = entire bar hidden.")]
        public bool ShowProgressBar = true;
        [Tooltip("Show the spinner Image. False = spinner hidden.")]
        public bool ShowSpinner = true;
        [Tooltip("If > 0, fires OnMinDisplayTimeElapsed after this many seconds post-Show. Advisory only — does not block transitions.")]
        public float MinDisplaySeconds = 0f;
    }
}
