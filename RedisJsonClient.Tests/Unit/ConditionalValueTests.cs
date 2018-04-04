using FluentAssertions;
using NUnit.Framework;

namespace RedisJsonClient.Tests
{
    public class ConditionalValueTests
    {
        [Test]
        public void HasValue_ReturnFalse_WhenNoValue()
        {
            ConditionalValue<bool>.NoValue.HasValue.Should().BeFalse();
        }

        [Test]
        public void ConditionalValue_IsConvertedToBooleanFalse_WhenNoValue()
        {
            bool value = new ConditionalValue<string>();
            value.Should().BeFalse();
        }

        [Test]
        public void Value_IsReturned_WhenPresent()
        {
            new ConditionalValue<int>(123).Value.Should().Be(123);
        }

        [Test]
        public void ConditionalValue_IsConvertedToBoolenTrue_WhenValuePresent()
        {
            bool value = ConditionalValue.From("string");
            value.Should().BeTrue();
        }

        public class MyTestClass
        {
        }

        [Test]
        public void ConditionalValue_IsConvertedToTheSameInstance_WhenValuePresent()
        {
            var myValue = new MyTestClass();
            MyTestClass conditionalValue = ConditionalValue.From(myValue);

            (myValue == conditionalValue).Should().BeTrue();
        }

        [Test]
        public void _ConditionalValue_IsConvertedToTheSameInstance_WhenValuePresent()
        {
            var myValue = new MyTestClass();
            MyTestClass conditionalValue = ConditionalValue.From(new MyTestClass());

            (myValue == conditionalValue).Should().BeFalse();
        }

        [Test]
        public void GetValueOrDefault_ReturnsValue_WhenPresent()
        {
            ConditionalValue.From("value").GetValueOrDefault("default").Should().Be("value");
        }

        [Test]
        public void GetValueOrDefault_ReturnsDefault_WhenNoValuePresent()
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            ConditionalValue<string>.NoValue.GetValueOrDefault("default").Should().Be("default");
        }
    }
}
