using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Catalog.Tutorial;
using NUnit.Framework;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class TutorialPopupTests
    {
        private GameObject _go;
        private TutorialPopup _popup;
        private int _stepChangedCount;
        private int _nextCount;
        private int _previousCount;
        private int _skipCount;
        private int _completedCount;
        private int _dismissedCount;
        private int _lastStepChangedIndex;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TutorialPopup_Test");
            _go.AddComponent<CanvasGroup>();
            _go.AddComponent<UIAnimTutorialPopup>();
            _popup = _go.AddComponent<TutorialPopup>();
            ResetCounters();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        private void ResetCounters()
        {
            _stepChangedCount = 0;
            _nextCount = 0;
            _previousCount = 0;
            _skipCount = 0;
            _completedCount = 0;
            _dismissedCount = 0;
            _lastStepChangedIndex = -1;
        }

        private void SubscribeCounters()
        {
            _popup.OnStepChanged += i => { _stepChangedCount++; _lastStepChangedIndex = i; };
            _popup.OnNext += _ => _nextCount++;
            _popup.OnPrevious += _ => _previousCount++;
            _popup.OnSkip += () => _skipCount++;
            _popup.OnCompleted += () => _completedCount++;
            _popup.OnDismissed += () => _dismissedCount++;
        }

        private static TutorialPopupData ThreeStepData(int startIndex = 0, bool loop = false)
        {
            return new TutorialPopupData
            {
                Steps = new List<TutorialStep>
                {
                    new TutorialStep { Title = "S0", Body = "B0" },
                    new TutorialStep { Title = "S1", Body = "B1" },
                    new TutorialStep { Title = "S2", Body = "B2" }
                },
                StartIndex = startIndex,
                LoopBackToFirst = loop
            };
        }

        [Test]
        public void Bind_With_Null_Data_Uses_Defaults_Without_Errors()
        {
            _popup.Bind(null);
            Assert.IsFalse(_popup.IsDismissing);
            Assert.AreEqual(0, _popup.StepCount);
            Assert.AreEqual(0, _popup.CurrentIndex);
            Assert.IsTrue(_popup.IsFirstStep);
        }

        [Test]
        public void Bind_With_Steps_Sets_Start_Index_And_Counts()
        {
            _popup.Bind(ThreeStepData(startIndex: 1));
            Assert.AreEqual(3, _popup.StepCount);
            Assert.AreEqual(1, _popup.CurrentIndex);
            Assert.IsFalse(_popup.IsFirstStep);
            Assert.IsFalse(_popup.IsLastStep);
        }

        [Test]
        public void GoNext_Advances_Index_And_Fires_StepChanged()
        {
            _popup.Bind(ThreeStepData());
            SubscribeCounters();
            _popup.GoNext();
            Assert.AreEqual(1, _popup.CurrentIndex);
            Assert.AreEqual(1, _stepChangedCount);
            Assert.AreEqual(1, _lastStepChangedIndex);
            Assert.AreEqual(1, _nextCount);
            Assert.AreEqual(0, _completedCount, "Completed must not fire on intermediate Next.");
        }

        [Test]
        public void GoNext_On_Last_Step_Fires_Completed_And_Dismisses()
        {
            _popup.Bind(ThreeStepData(startIndex: 2));
            SubscribeCounters();
            Assert.IsTrue(_popup.IsLastStep);
            _popup.GoNext();
            Assert.AreEqual(1, _completedCount, "Completed must fire once on last-step Next.");
            Assert.AreEqual(0, _nextCount, "OnNext must NOT fire when completing.");
            Assert.AreEqual(1, _dismissedCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void GoNext_On_Last_Step_With_Loop_Wraps_To_First()
        {
            _popup.Bind(ThreeStepData(startIndex: 2, loop: true));
            SubscribeCounters();
            _popup.GoNext();
            Assert.AreEqual(0, _popup.CurrentIndex, "Loop must wrap to index 0.");
            Assert.AreEqual(0, _completedCount, "Completed must NOT fire when looping.");
            Assert.AreEqual(1, _nextCount);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void GoPrevious_On_First_Step_Is_Ignored()
        {
            _popup.Bind(ThreeStepData(startIndex: 0));
            SubscribeCounters();
            _popup.GoPrevious();
            Assert.AreEqual(0, _popup.CurrentIndex);
            Assert.AreEqual(0, _previousCount);
            Assert.AreEqual(0, _stepChangedCount);
        }

        [Test]
        public void GoPrevious_Decrements_Index()
        {
            _popup.Bind(ThreeStepData(startIndex: 2));
            SubscribeCounters();
            _popup.GoPrevious();
            Assert.AreEqual(1, _popup.CurrentIndex);
            Assert.AreEqual(1, _previousCount);
            Assert.AreEqual(1, _stepChangedCount);
            Assert.AreEqual(1, _lastStepChangedIndex);
        }

        [Test]
        public void Back_Press_Triggers_Skip_And_Dismisses()
        {
            _popup.Bind(ThreeStepData(startIndex: 1));
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _skipCount, "Back press must trigger Skip (not Previous).");
            Assert.AreEqual(0, _previousCount);
            Assert.AreEqual(1, _dismissedCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Back_Press_While_Dismissing_Is_Ignored()
        {
            _popup.Bind(ThreeStepData());
            SubscribeCounters();
            _popup.OnBackPressed();
            _popup.OnBackPressed();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _skipCount, "Subsequent back presses must be ignored once dismissing.");
        }

        [Test]
        public void Bind_Resets_Event_Listeners()
        {
            _popup.Bind(ThreeStepData());
            SubscribeCounters();
            _popup.GoNext();
            var beforeNext = _nextCount;
            _popup.Bind(ThreeStepData());
            _popup.GoNext();
            Assert.AreEqual(beforeNext, _nextCount, "Listeners attached before re-Bind must NOT receive events after re-Bind.");
        }

        [Test]
        public void Backdrop_With_TapToAdvance_Calls_GoNext()
        {
            var data = ThreeStepData();
            data.TapToAdvance = true;
            _popup.Bind(data);
            SubscribeCounters();
            _popup.HandleBackdrop();
            Assert.AreEqual(1, _popup.CurrentIndex, "TapToAdvance must advance the step.");
            Assert.AreEqual(1, _nextCount);
            Assert.AreEqual(0, _skipCount, "TapToAdvance must NOT trigger Skip.");
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Backdrop_With_CloseOnBackdrop_Calls_Skip()
        {
            var data = ThreeStepData();
            data.CloseOnBackdrop = true;
            _popup.Bind(data);
            SubscribeCounters();
            _popup.HandleBackdrop();
            Assert.AreEqual(1, _skipCount, "CloseOnBackdrop must route to Skip.");
            Assert.AreEqual(1, _dismissedCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Backdrop_With_Both_Flags_TapToAdvance_Wins()
        {
            var data = ThreeStepData();
            data.TapToAdvance = true;
            data.CloseOnBackdrop = true;
            _popup.Bind(data);
            SubscribeCounters();
            _popup.HandleBackdrop();
            Assert.AreEqual(1, _popup.CurrentIndex, "TapToAdvance must win over CloseOnBackdrop.");
            Assert.AreEqual(1, _nextCount);
            Assert.AreEqual(0, _skipCount);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void StartIndex_Negative_Is_Clamped_To_Zero()
        {
            _popup.Bind(ThreeStepData(startIndex: -5));
            Assert.AreEqual(0, _popup.CurrentIndex, "Negative StartIndex must clamp to 0.");
            Assert.IsTrue(_popup.IsFirstStep);
        }

        [Test]
        public void StartIndex_Beyond_Bounds_Is_Clamped_To_Last()
        {
            _popup.Bind(ThreeStepData(startIndex: 99));
            Assert.AreEqual(2, _popup.CurrentIndex, "Out-of-bounds StartIndex must clamp to StepCount - 1.");
            Assert.IsTrue(_popup.IsLastStep);
        }

        [Test]
        public void CompleteTutorial_Public_Api_Forces_Completion_Mid_Flow()
        {
            _popup.Bind(ThreeStepData(startIndex: 1));
            SubscribeCounters();
            _popup.CompleteTutorial();
            Assert.AreEqual(1, _completedCount, "CompleteTutorial must fire OnCompleted regardless of step.");
            Assert.AreEqual(1, _dismissedCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Rebind_Resets_CurrentIndex_From_New_StartIndex()
        {
            _popup.Bind(ThreeStepData(startIndex: 2));
            Assert.AreEqual(2, _popup.CurrentIndex);
            _popup.Bind(ThreeStepData(startIndex: 0));
            Assert.AreEqual(0, _popup.CurrentIndex, "Re-Bind must apply the new StartIndex.");
            Assert.IsTrue(_popup.IsFirstStep);
            Assert.IsFalse(_popup.IsDismissing, "Re-Bind must reset IsDismissing.");
        }

        [Test]
        public void GoNext_With_Empty_Steps_Is_NoOp()
        {
            _popup.Bind(new TutorialPopupData());
            SubscribeCounters();
            _popup.GoNext();
            Assert.AreEqual(0, _nextCount, "GoNext on empty Steps must NOT fire OnNext.");
            Assert.AreEqual(0, _completedCount);
            Assert.AreEqual(0, _stepChangedCount);
        }

        [Test]
        public void GoPrevious_With_Empty_Steps_Is_NoOp()
        {
            _popup.Bind(new TutorialPopupData());
            SubscribeCounters();
            _popup.GoPrevious();
            Assert.AreEqual(0, _previousCount);
            Assert.AreEqual(0, _stepChangedCount);
        }
    }
}
