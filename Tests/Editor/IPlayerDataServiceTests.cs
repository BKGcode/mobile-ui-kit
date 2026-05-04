using System;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class IPlayerDataServiceTests
    {
        private IPlayerDataService _data;

        [SetUp]
        public void SetUp()
        {
            _data = new FakePlayerDataService();
        }

        [Test]
        public void GetInt_KeyAbsent_Returns_Default()
        {
            Assert.AreEqual(42, _data.GetInt("kfmui.test.absent", 42));
        }

        [Test]
        public void SetInt_Then_GetInt_Roundtrips_Value()
        {
            _data.SetInt("kfmui.test.coins", 1234);
            Assert.AreEqual(1234, _data.GetInt("kfmui.test.coins"));
        }

        [Test]
        public void GetFloat_KeyAbsent_Returns_Default()
        {
            Assert.AreEqual(0.75f, _data.GetFloat("kfmui.test.absent", 0.75f));
        }

        [Test]
        public void SetFloat_Then_GetFloat_Roundtrips_Value()
        {
            _data.SetFloat("kfmui.settings.musicVolume", 0.42f);
            Assert.AreEqual(0.42f, _data.GetFloat("kfmui.settings.musicVolume"));
        }

        [Test]
        public void GetString_KeyAbsent_Returns_Default()
        {
            Assert.AreEqual("en", _data.GetString("kfmui.test.absent", "en"));
        }

        [Test]
        public void SetString_Then_GetString_Roundtrips_Value()
        {
            _data.SetString("kfmui.settings.language", "es");
            Assert.AreEqual("es", _data.GetString("kfmui.settings.language"));
        }

        [Test]
        public void GetBool_KeyAbsent_Returns_Default()
        {
            Assert.IsTrue(_data.GetBool("kfmui.test.absent", true));
        }

        [Test]
        public void SetBool_Then_GetBool_Roundtrips_Value()
        {
            _data.SetBool("kfmui.settings.haptics", false);
            Assert.IsFalse(_data.GetBool("kfmui.settings.haptics", true));
        }

        [Test]
        public void Has_Returns_False_When_Key_Absent()
        {
            Assert.IsFalse(_data.Has("kfmui.test.absent"));
        }

        [Test]
        public void Has_Returns_True_After_Set()
        {
            _data.SetInt("kfmui.test.key", 1);
            Assert.IsTrue(_data.Has("kfmui.test.key"));
        }

        [Test]
        public void Has_Returns_False_After_Delete()
        {
            _data.SetInt("kfmui.test.key", 1);
            _data.Delete("kfmui.test.key");
            Assert.IsFalse(_data.Has("kfmui.test.key"));
        }

        [Test]
        public void Delete_Of_Absent_Key_Is_Silent_NoOp()
        {
            Assert.DoesNotThrow(() => _data.Delete("kfmui.test.absent"));
        }

        [Test]
        public void Save_Does_Not_Throw()
        {
            _data.SetInt("kfmui.test.key", 1);
            Assert.DoesNotThrow(() => _data.Save());
        }

        [Test]
        public void Reload_Clears_All_Keys()
        {
            _data.SetInt("kfmui.test.a", 1);
            _data.SetString("kfmui.test.b", "x");
            _data.Reload();
            Assert.IsFalse(_data.Has("kfmui.test.a"));
            Assert.IsFalse(_data.Has("kfmui.test.b"));
        }

        [Test]
        public void Type_Mismatch_On_Read_Throws_InvalidCastException()
        {
            _data.SetInt("kfmui.test.key", 1);
            Assert.Throws<InvalidCastException>(() => _data.GetString("kfmui.test.key"));
        }

        [Test]
        public void Null_Key_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _data.GetInt(null));
            Assert.Throws<ArgumentNullException>(() => _data.SetInt(null, 1));
            Assert.Throws<ArgumentNullException>(() => _data.Has(null));
            Assert.Throws<ArgumentNullException>(() => _data.Delete(null));
        }
    }
}
