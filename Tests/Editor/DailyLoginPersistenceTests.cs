using System;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class DailyLoginPersistenceTests
    {
        private FakePlayerDataService _pd;

        [SetUp]
        public void SetUp()
        {
            _pd = new FakePlayerDataService();
        }

        [Test]
        public void Load_With_Null_PlayerData_Returns_State_Unchanged()
        {
            var state = new DailyLoginState
            {
                CurrentDay = 3,
                DoubledToday = true,
                LastClaimUtc = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            };
            DailyLoginPersistence.Load(null, ref state);
            Assert.AreEqual(3, state.CurrentDay);
            Assert.IsTrue(state.DoubledToday);
            Assert.AreEqual(new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), state.LastClaimUtc);
        }

        [Test]
        public void Save_With_Null_PlayerData_Is_NoOp()
        {
            var state = new DailyLoginState { CurrentDay = 5 };
            Assert.DoesNotThrow(() => DailyLoginPersistence.Save(null, state));
        }

        [Test]
        public void Save_Then_Load_Roundtrips_All_Fields()
        {
            var saved = new DailyLoginState
            {
                CurrentDay = 7,
                DoubledToday = true,
                LastClaimUtc = new DateTime(2026, 5, 4, 12, 30, 0, DateTimeKind.Utc),
            };
            DailyLoginPersistence.Save(_pd, saved);

            var loaded = new DailyLoginState();
            DailyLoginPersistence.Load(_pd, ref loaded);
            Assert.AreEqual(7, loaded.CurrentDay);
            Assert.IsTrue(loaded.DoubledToday);
            Assert.AreEqual(saved.LastClaimUtc, loaded.LastClaimUtc);
        }

        [Test]
        public void Save_Default_LastClaimUtc_Stores_Empty_String()
        {
            var state = new DailyLoginState { CurrentDay = 1, LastClaimUtc = default };
            DailyLoginPersistence.Save(_pd, state);
            Assert.AreEqual("", _pd.GetString(DailyLoginPersistence.KeyLastClaimUtc));
        }

        [Test]
        public void Load_With_Empty_LastClaimUtc_Returns_Default_DateTime()
        {
            _pd.SetInt(DailyLoginPersistence.KeyCurrentDay, 2);
            _pd.SetString(DailyLoginPersistence.KeyLastClaimUtc, "");
            _pd.SetBool(DailyLoginPersistence.KeyDoubledToday, false);
            var state = new DailyLoginState();
            DailyLoginPersistence.Load(_pd, ref state);
            Assert.AreEqual(default(DateTime), state.LastClaimUtc);
            Assert.AreEqual(2, state.CurrentDay);
        }
    }
}
