using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class IUILocalizationServiceTests
    {
        private static IReadOnlyList<string> Available => new[] { "en", "es", "fr" };

        [Test]
        public void Constructor_With_Empty_AvailableCodes_Throws_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new FakeLocalizationService("en", new string[0]));
        }

        [Test]
        public void Constructor_With_CurrentCode_Not_In_Available_Throws_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new FakeLocalizationService("de", Available));
        }

        [Test]
        public void CurrentLanguage_Returns_Constructor_Value()
        {
            var svc = new FakeLocalizationService("es", Available);
            Assert.AreEqual("es", svc.CurrentLanguage);
        }

        [Test]
        public void AvailableLanguages_Mirrors_Constructor_Input()
        {
            var svc = new FakeLocalizationService("en", Available);
            CollectionAssert.AreEqual(Available, svc.AvailableLanguages);
        }

        [Test]
        public void SetLanguage_Happy_Path_Raises_OnLanguageChanged()
        {
            var svc = new FakeLocalizationService("en", Available);
            var raised = false;
            svc.OnLanguageChanged += _ => raised = true;
            svc.SetLanguage("es");
            Assert.IsTrue(raised);
        }

        [Test]
        public void SetLanguage_To_Same_Code_Is_NoOp_No_Event()
        {
            var svc = new FakeLocalizationService("en", Available);
            var callCount = 0;
            svc.OnLanguageChanged += _ => callCount++;
            svc.SetLanguage("en");
            Assert.AreEqual(0, callCount);
            Assert.AreEqual("en", svc.CurrentLanguage);
        }

        [Test]
        public void SetLanguage_With_Unknown_Code_Throws_ArgumentException()
        {
            var svc = new FakeLocalizationService("en", Available);
            Assert.Throws<ArgumentException>(() => svc.SetLanguage("de"));
        }

        [Test]
        public void OnLanguageChanged_Delivers_The_New_Code()
        {
            var svc = new FakeLocalizationService("en", Available);
            string received = null;
            svc.OnLanguageChanged += code => received = code;
            svc.SetLanguage("fr");
            Assert.AreEqual("fr", received);
            Assert.AreEqual("fr", svc.CurrentLanguage);
        }

        [Test]
        public void Multiple_Subscribers_All_Receive_Event()
        {
            var svc = new FakeLocalizationService("en", Available);
            var aCalls = 0;
            var bCalls = 0;
            svc.OnLanguageChanged += _ => aCalls++;
            svc.OnLanguageChanged += _ => bCalls++;
            svc.SetLanguage("es");
            Assert.AreEqual(1, aCalls);
            Assert.AreEqual(1, bCalls);
        }

        [Test]
        public void Unsubscribe_Stops_Event_Delivery()
        {
            var svc = new FakeLocalizationService("en", Available);
            var callCount = 0;
            Action<string> handler = _ => callCount++;
            svc.OnLanguageChanged += handler;
            svc.SetLanguage("es");
            svc.OnLanguageChanged -= handler;
            svc.SetLanguage("fr");
            Assert.AreEqual(1, callCount);
        }
    }
}
