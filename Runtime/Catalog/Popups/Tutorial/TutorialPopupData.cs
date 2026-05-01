using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Tutorial
{
    [Serializable]
    public class TutorialStep
    {
        public string Title = string.Empty;
        [TextArea(2, 5)] public string Body = string.Empty;
        public Sprite Image;
    }

    [Serializable]
    public class TutorialPopupData
    {
        public List<TutorialStep> Steps = new List<TutorialStep>();

        [Tooltip("Index the popup opens on. Clamped to [0, Steps.Count - 1].")]
        public int StartIndex = 0;

        [Header("Navigation")]
        public bool ShowPrevious = true;
        public bool ShowSkip = true;

        [Tooltip("If true, GoNext on the last step wraps to index 0 instead of firing OnCompleted.")]
        public bool LoopBackToFirst = false;

        [Header("Backdrop behaviour")]
        public bool CloseOnBackdrop = false;

        [Tooltip("If true, tapping the backdrop advances to the next step (overrides CloseOnBackdrop on backdrop tap).")]
        public bool TapToAdvance = false;

        [Header("Labels")]
        public string NextLabel = "Next";
        public string PreviousLabel = "Back";
        public string SkipLabel = "Skip";
        public string DoneLabel = "Got it!";
    }
}
