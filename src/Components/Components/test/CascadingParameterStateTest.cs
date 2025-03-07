// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.Binding;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Test.Helpers;
using Moq;

namespace Microsoft.AspNetCore.Components;

public class CascadingParameterStateTest
{
    [Fact]
    public void FindCascadingParameters_IfHasNoParameters_ReturnsEmpty()
    {
        // Arrange
        var componentState = CreateComponentState(new ComponentWithNoParams());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(componentState);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindCascadingParameters_IfHasNoCascadingParameters_ReturnsEmpty()
    {
        // Arrange
        var componentState = CreateComponentState(new ComponentWithNoCascadingParams());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(componentState);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindCascadingParameters_IfHasNoAncestors_ReturnsEmpty()
    {
        // Arrange
        var componentState = CreateComponentState(new ComponentWithCascadingParams());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(componentState);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindCascadingParameters_IfHasNoMatchesInAncestors_ReturnsEmpty()
    {
        // Arrange: Build the ancestry list
        var states = CreateAncestry(
            new ComponentWithNoParams(),
            CreateCascadingValueComponent("Hello"),
            new ComponentWithNoParams(),
            new ComponentWithCascadingParams());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindCascadingParameters_IfHasPartialMatchesInAncestors_ReturnsMatches()
    {
        // Arrange
        var states = CreateAncestry(
            new ComponentWithNoParams(),
            CreateCascadingValueComponent(new ValueType2()),
            new ComponentWithNoParams(),
            new ComponentWithCascadingParams());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Collection(result, match =>
        {
            Assert.Equal(nameof(ComponentWithCascadingParams.CascadingParam2), match.ParameterInfo.PropertyName);
            Assert.Same(states[1].Component, match.ValueSupplier);
        });
    }

    [Fact]
    public void FindCascadingParameters_IfHasMultipleMatchesInAncestors_ReturnsMatches()
    {
        // Arrange
        var states = CreateAncestry(
            new ComponentWithNoParams(),
            CreateCascadingValueComponent(new ValueType2()),
            new ComponentWithNoParams(),
            CreateCascadingValueComponent(new ValueType1()),
            new ComponentWithCascadingParams());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Collection(result.OrderBy(x => x.ParameterInfo.PropertyName),
            match =>
            {
                Assert.Equal(nameof(ComponentWithCascadingParams.CascadingParam1), match.ParameterInfo.PropertyName);
                Assert.Same(states[3].Component, match.ValueSupplier);
            },
            match =>
            {
                Assert.Equal(nameof(ComponentWithCascadingParams.CascadingParam2), match.ParameterInfo.PropertyName);
                Assert.Same(states[1].Component, match.ValueSupplier);
            });
    }

    [Fact]
    public void FindCascadingParameters_InheritedParameters_ReturnsMatches()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent(new ValueType1()),
            CreateCascadingValueComponent(new ValueType3()),
            new ComponentWithInheritedCascadingParams());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Collection(result.OrderBy(x => x.ParameterInfo.PropertyName),
            match =>
            {
                Assert.Equal(nameof(ComponentWithCascadingParams.CascadingParam1), match.ParameterInfo.PropertyName);
                Assert.Same(states[0].Component, match.ValueSupplier);
            },
            match =>
            {
                Assert.Equal(nameof(ComponentWithInheritedCascadingParams.CascadingParam3), match.ParameterInfo.PropertyName);
                Assert.Same(states[1].Component, match.ValueSupplier);
            });
    }

    [Fact]
    public void FindCascadingParameters_ComponentRequestsBaseType_ReturnsMatches()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent(new CascadingValueTypeDerivedClass()),
            new ComponentWithGenericCascadingParam<CascadingValueTypeBaseClass>());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Collection(result, match =>
        {
            Assert.Equal(nameof(ComponentWithGenericCascadingParam<object>.LocalName), match.ParameterInfo.PropertyName);
            Assert.Same(states[0].Component, match.ValueSupplier);
        });
    }

    [Fact]
    public void FindCascadingParameters_ComponentRequestsImplementedInterface_ReturnsMatches()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent(new CascadingValueTypeDerivedClass()),
            new ComponentWithGenericCascadingParam<ICascadingValueTypeDerivedClassInterface>());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Collection(result, match =>
        {
            Assert.Equal(nameof(ComponentWithGenericCascadingParam<object>.LocalName), match.ParameterInfo.PropertyName);
            Assert.Same(states[0].Component, match.ValueSupplier);
        });
    }

    [Fact]
    public void FindCascadingParameters_ComponentRequestsDerivedType_ReturnsEmpty()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent(new CascadingValueTypeBaseClass()),
            new ComponentWithGenericCascadingParam<CascadingValueTypeDerivedClass>());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindCascadingParameters_TypeAssignmentIsValidForNullValue_ReturnsMatches()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent((CascadingValueTypeDerivedClass)null),
            new ComponentWithGenericCascadingParam<CascadingValueTypeBaseClass>());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Collection(result, match =>
        {
            Assert.Equal(nameof(ComponentWithGenericCascadingParam<object>.LocalName), match.ParameterInfo.PropertyName);
            Assert.Same(states[0].Component, match.ValueSupplier);
        });
    }

    [Fact]
    public void FindCascadingParameters_TypeAssignmentIsInvalidForNullValue_ReturnsEmpty()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent((object)null),
            new ComponentWithGenericCascadingParam<ValueType1>());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindCascadingParameters_SupplierSpecifiesNameButConsumerDoesNot_ReturnsEmpty()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent(new ValueType1(), "MatchOnName"),
            new ComponentWithCascadingParams());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindCascadingParameters_ConsumerSpecifiesNameButSupplierDoesNot_ReturnsEmpty()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent(new ValueType1()),
            new ComponentWithNamedCascadingParam());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindCascadingParameters_MismatchingNameButMatchingType_ReturnsEmpty()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent(new ValueType1(), "MismatchName"),
            new ComponentWithNamedCascadingParam());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindCascadingParameters_MatchingNameButMismatchingType_ReturnsEmpty()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent(new ValueType2(), "MatchOnName"),
            new ComponentWithNamedCascadingParam());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindCascadingParameters_MatchingNameAndType_ReturnsMatches()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent(new ValueType1(), "matchonNAME"), // To show it's case-insensitive
            new ComponentWithNamedCascadingParam());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Collection(result, match =>
        {
            Assert.Equal(nameof(ComponentWithNamedCascadingParam.SomeLocalName), match.ParameterInfo.PropertyName);
            Assert.Same(states[0].Component, match.ValueSupplier);
        });
    }

    [Fact]
    public void FindCascadingParameters_MultipleMatchingAncestors_ReturnsClosestMatches()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent(new ValueType1()),
            CreateCascadingValueComponent(new ValueType2()),
            CreateCascadingValueComponent(new ValueType1()),
            CreateCascadingValueComponent(new ValueType2()),
            new ComponentWithCascadingParams());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Collection(result.OrderBy(x => x.ParameterInfo.PropertyName),
            match =>
            {
                Assert.Equal(nameof(ComponentWithCascadingParams.CascadingParam1), match.ParameterInfo.PropertyName);
                Assert.Same(states[2].Component, match.ValueSupplier);
            },
            match =>
            {
                Assert.Equal(nameof(ComponentWithCascadingParams.CascadingParam2), match.ParameterInfo.PropertyName);
                Assert.Same(states[3].Component, match.ValueSupplier);
            });
    }

    [Fact]
    public void FindCascadingParameters_CanOverrideNonNullValueWithNull()
    {
        // Arrange
        var states = CreateAncestry(
            CreateCascadingValueComponent(new ValueType1()),
            CreateCascadingValueComponent((ValueType1)null),
            new ComponentWithCascadingParams());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        Assert.Collection(result.OrderBy(x => x.ParameterInfo.PropertyName),
            match =>
            {
                Assert.Equal(nameof(ComponentWithCascadingParams.CascadingParam1), match.ParameterInfo.PropertyName);
                Assert.Same(states[1].Component, match.ValueSupplier);
                Assert.Null(match.ValueSupplier.GetCurrentValue(match.ParameterInfo));
            });
    }

    [Fact]
    public void FindCascadingParameters_HandlesSupplyParameterFromFormValues()
    {
        // Arrange
        var provider = new TestCascadingFormModelBindingProvider
        {
            FormName = "",
            CurrentValue = "some value",
        };
        var cascadingModelBinder = new CascadingModelBinder
        {
            ModelBindingProviders = new[] { provider },
            Navigation = Mock.Of<NavigationManager>(),
            Name = ""
        };

        cascadingModelBinder.UpdateBindingInformation("https://localhost/");

        var states = CreateAncestry(
            cascadingModelBinder,
            new FormParametersComponent());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        var supplier = Assert.Single(result);
        Assert.Equal(cascadingModelBinder, supplier.ValueSupplier);
    }

    [Fact]
    public void FindCascadingParameters_HandlesSupplyParameterFromFormValues_WithName()
    {
        // Arrange
        var provider = new TestCascadingFormModelBindingProvider
        {
            FormName = "some-name",
            CurrentValue = "some value",
        };
        var cascadingModelBinder = new CascadingModelBinder
        {
            ModelBindingProviders = new[] { provider },
            Navigation = new TestNavigationManager(),
            Name = "some-name"
        };

        cascadingModelBinder.UpdateBindingInformation("https://localhost/");

        var states = CreateAncestry(
            cascadingModelBinder,
            new FormParametersComponentWithName());

        // Act
        var result = CascadingParameterState.FindCascadingParameters(states.Last());

        // Assert
        var supplier = Assert.Single(result);
        Assert.Equal(cascadingModelBinder, supplier.ValueSupplier);
    }

    static ComponentState[] CreateAncestry(params IComponent[] components)
    {
        var result = new ComponentState[components.Length];

        for (var i = 0; i < components.Length; i++)
        {
            result[i] = CreateComponentState(
                components[i],
                i == 0 ? null : result[i - 1]);
        }

        return result;
    }

    static ComponentState CreateComponentState(
        IComponent component, ComponentState parentComponentState = null)
    {
        return new ComponentState(new TestRenderer(), 0, component, parentComponentState);
    }

    static CascadingValue<T> CreateCascadingValueComponent<T>(T value, string name = null)
    {
        var supplier = new CascadingValue<T>();
        var renderer = new TestRenderer();
        supplier.Attach(new RenderHandle(renderer, 0));

        var supplierParams = new Dictionary<string, object>
            {
                { "Value", value }
            };

        if (name != null)
        {
            supplierParams.Add("Name", name);
        }

        renderer.Dispatcher.InvokeAsync((Action)(() => supplier.SetParametersAsync(ParameterView.FromDictionary(supplierParams))));
        return supplier;
    }

    class ComponentWithNoParams : TestComponentBase
    {
    }

    class FormParametersComponent : TestComponentBase
    {
        [SupplyParameterFromForm] public string FormParameter { get; set; }
    }

    class FormParametersComponentWithName : TestComponentBase
    {
        [SupplyParameterFromForm(Handler = "some-name")] public string FormParameter { get; set; }
    }

    class ComponentWithNoCascadingParams : TestComponentBase
    {
        [Parameter] public bool SomeRegularParameter { get; set; }
    }

    class ComponentWithCascadingParams : TestComponentBase
    {
        [Parameter] public bool RegularParam { get; set; }
        [CascadingParameter] internal ValueType1 CascadingParam1 { get; set; }
        [CascadingParameter] internal ValueType2 CascadingParam2 { get; set; }
    }

    class ComponentWithInheritedCascadingParams : ComponentWithCascadingParams
    {
        [CascadingParameter] internal ValueType3 CascadingParam3 { get; set; }
    }

    class ComponentWithGenericCascadingParam<T> : TestComponentBase
    {
        [CascadingParameter] internal T LocalName { get; set; }
    }

    class ComponentWithNamedCascadingParam : TestComponentBase
    {
        [CascadingParameter(Name = "MatchOnName")]
        internal ValueType1 SomeLocalName { get; set; }
    }

    class TestComponentBase : IComponent
    {
        public void Attach(RenderHandle renderHandle)
            => throw new NotImplementedException();

        public Task SetParametersAsync(ParameterView parameters)
            => throw new NotImplementedException();
    }

    class ValueType1 { }
    class ValueType2 { }
    class ValueType3 { }

    class CascadingValueTypeBaseClass { }
    class CascadingValueTypeDerivedClass : CascadingValueTypeBaseClass, ICascadingValueTypeDerivedClassInterface { }
    interface ICascadingValueTypeDerivedClassInterface { }

    private class TestCascadingFormModelBindingProvider : CascadingModelBindingProvider
    {
        public required string FormName { get; init; }

        public required string CurrentValue { get; init; }

        protected internal override bool AreValuesFixed => true;

        protected internal override bool CanSupplyValue(ModelBindingContext bindingContext, in CascadingParameterInfo parameterInfo)
            => string.Equals(bindingContext.Name, FormName, StringComparison.Ordinal);

        protected internal override object GetCurrentValue(ModelBindingContext bindingContext, in CascadingParameterInfo parameterInfo)
            => CurrentValue;

        protected internal override bool SupportsCascadingParameterAttributeType(Type attributeType)
            => attributeType == typeof(SupplyParameterFromFormAttribute);

        protected internal override bool SupportsParameterType(Type parameterType)
            => parameterType == typeof(string);
    }

    class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager()
        {
            Initialize("https://localhost:85/subdir/", "https://localhost:85/subdir/path?query=value#hash");
        }
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class SupplyParameterFromFormAttribute : CascadingParameterAttributeBase
{
    /// <summary>
    /// Gets or sets the name for the parameter. The name is used to match
    /// the form data and decide whether or not the value needs to be bound.
    /// </summary>
    public override string Name { get; set; }

    /// <summary>
    /// Gets or sets the name for the handler. The name is used to match
    /// the form data and decide whether or not the value needs to be bound.
    /// </summary>
    public string Handler { get; set; }
}
