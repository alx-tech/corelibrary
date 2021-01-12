using Xunit;
using static LeanCode.ContractsGenerator.Tests.Dart.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    public class ContractsGeneratorTests_ClassGeneration
    {
        [Fact]
        public void Private_class_is_not_resolved()
        {
            var generator = CreateDartGeneratorFromNamespace("private class TestClass { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.DoesNotContain("TestClass", contracts);
        }

        [Fact]
        public void Protected_class_is_not_resolved()
        {
            var generator = CreateDartGeneratorFromNamespace("private class TestClass { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.DoesNotContain("TestClass", contracts);
        }

        [Fact]
        public void Class_with_no_access_modifier_is_not_resolved()
        {
            var generator = CreateDartGeneratorFromNamespace("class TestClass { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.DoesNotContain("TestClass", contracts);
        }

        [Fact]
        public void Public_class_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass", contracts);
        }

        [Fact]
        public void Generic_class_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass<T> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass<T>", contracts);
        }

        [Fact]
        public void Generic_class_with_constraints_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public interface IRemoteCommand { } public class TestClass<T> where T: IList<IRemoteCommand> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass<T implements List<IRemoteCommand>", contracts);
        }

        [Fact]
        public void Class_inheritance_from_interface_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public interface IRemoteCommand { } public class TestClass : IRemoteCommand {}");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass implements IRemoteCommand", contracts);
        }

        [Fact]
        public void Generic_class_inheritance_from_interface_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public interface IRemoteCommand<T> { } public class TestClass<T> : IRemoteCommand<T> {}");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass<T> implements IRemoteCommand<T>", contracts);
        }

        [Fact]
        public void Deep_inheritance_from_interface_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public interface IRemoteQuery<T> { } public class TestClass : IRemoteQuery<List<int>> {}");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass implements IRemoteQuery<List<int>>", contracts);
        }

        [Fact]
        public void Deep_generic_inheritance_from_interface_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public interface IRemoteQuery<T> { } public class TestClass<T> : IRemoteQuery<T> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass<T> implements IRemoteQuery<T>", contracts);
        }

        [Fact]
        public void Static_class_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public static class ErrorCodes { public const int Invalid = 1; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("invalid = 1", contracts);
            Assert.Contains("ErrorCodes {", contracts);
        }

        [Fact]
        public void Order_of_classes_is_alphanumeric()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass2 { } public class TestClass1 { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass1", contracts);
            Assert.Contains("class TestClass2", contracts);

            var test1Index = contracts.IndexOf("class TestClass1");
            var test2Index = contracts.IndexOf("class TestClass2");

            Assert.True(test1Index < test2Index);
        }

        [Fact]
        public void Constructors_are_before_other_members()
        {
            var generator = CreateDartGeneratorFromNamespace(
@"
namespace N
{
    public class DTO {}

    public class A : IRemoteQuery<DTO>
    {
        public int Field { get; set; }
        public const int ConstField = 1;
    }
}
");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("A();", contracts);
            Assert.Contains("factory A.fromJson(", contracts);
            Assert.Contains("Field", contracts);
            Assert.Contains("constField", contracts);
            Assert.Contains("getFullName", contracts);
            Assert.Contains("resultFactory(", contracts);
            Assert.Contains("toJson(", contracts);

            var constructorIdx = contracts.IndexOf("A();");
            var factoryIdx = contracts.IndexOf("factory A.fromJson(");

            Assert.True(constructorIdx < factoryIdx);

            var testIdx = contracts.IndexOf("Field");
            Assert.True(constructorIdx < testIdx);
            Assert.True(factoryIdx < testIdx);

            testIdx = contracts.IndexOf("constField");
            Assert.True(constructorIdx < testIdx);
            Assert.True(factoryIdx < testIdx);

            testIdx = contracts.IndexOf("getFullName");
            Assert.True(constructorIdx < testIdx);
            Assert.True(factoryIdx < testIdx);

            testIdx = contracts.IndexOf("resultFactory(");
            Assert.True(constructorIdx < testIdx);
            Assert.True(factoryIdx < testIdx);

            testIdx = contracts.IndexOf("toJson(");
            Assert.True(constructorIdx < testIdx);
            Assert.True(factoryIdx < testIdx);
        }

        [Fact]
        public void Classes_having_the_same_names_are_supplied_with_minimal_namespace_name()
        {
            var generator = CreateDartGenerator(
@"namespace Aa.Bb.Cc
{
    public class Class { }
}
namespace Aaa.Bbb.Cc
{
    public class Class { }
}");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            var firstClass =
@"@JsonSerializable()
class CcBbClass {
    CcBbClass();

    factory CcBbClass.fromJson(Map<String, dynamic> json) => _$CcBbClassFromJson(json);

    Map<String, dynamic> toJson() => _$CcBbClassToJson(this);
}";
            Assert.Contains(firstClass, contracts);

            var secondClass =
@"@JsonSerializable()
class CcBbbClass {
    CcBbbClass();

    factory CcBbbClass.fromJson(Map<String, dynamic> json) => _$CcBbbClassFromJson(json);

    Map<String, dynamic> toJson() => _$CcBbbClassToJson(this);
}";
            Assert.Contains(secondClass, contracts);
        }

        [Fact]
        public void Class_inheritance_from_interface_other_than_cqrs_is_ignored()
        {
            var generator = CreateDartGeneratorFromNamespace("public interface ISomething { } public interface IRemoteCommand {} public class TestClass : IRemoteCommand, ISomething {}");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass implements IRemoteCommand", contracts);
        }

        [Fact]
        public void Class_translates_keyword_field_names_by_adding_underscore()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass { public int New { get; set; } }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("int new_;", contracts);
        }

        [Fact]
        public void Class_translates_normal_field_names_to_lower_case()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass { public int NotNew { get; set; } }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("int notNew;", contracts);
        }
    }
}
