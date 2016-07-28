﻿namespace Nancy.Tests.Unit.Json
{
    using System.Globalization;
    using Nancy.Json.Simple;
    using Xunit;

    public class SimpleJsonFixture
    {
        [Fact]
        public void String_dictionary_values_are_Json_serialized_as_strings()
        {
            //Given
            dynamic value = "42";
            var input = new DynamicDictionaryValue(value);

            //When
            var actual = SimpleJson.SerializeObject(input, new NancySerializationStrategy());

            //Then
            actual.ShouldEqual(@"""42""");
        }

        [Fact]
        public void Integer_dictionary_values_are_Json_serialized_as_integers()
        {
            //Given
            dynamic value = 42;
            var input = new DynamicDictionaryValue(value);

            //When
            var actual = SimpleJson.SerializeObject(input, new NancySerializationStrategy());

            //Then
            actual.ShouldEqual(@"42");
        }

        [Fact]
        public void Should_serialize_enum_to_string()
        {
            //Given
            var model = new ModelTest { EnumModel = TestEnum.Freddy };

            //When
            var result = SimpleJson.SerializeObject(model, new NancySerializationStrategy());

            //Then
            result.ShouldEqual("{\"enumModel\":\"Freddy\"}");
        }

        [Fact]
        public void Sho9uld_deserialize_string_to_enum()
        {
            //Given
            var json = "{\"enumModel\":\"Freddy\"}";

            //When
            var result = SimpleJson.DeserializeObject<ModelTest>(json, new NancySerializationStrategy(), DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

            //Then
            result.EnumModel.ShouldEqual(TestEnum.Freddy);
        }

        public class ModelTest
        {
            public TestEnum EnumModel { get; set; }
        }
        public enum TestEnum
        {
            Rod = 1,
            Jane = 2,
            Freddy = 3
        }
    }
}
