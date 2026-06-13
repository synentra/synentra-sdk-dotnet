using Synentra.Client.Models.Policies;

namespace Synentra.Client.UnitTests.Models;

public sealed class PolicyRuleTests
{
    [Fact]
    public void PolicyRule_DefaultValues()
    {
        var rule = new PolicyRule();

        rule.Name.Should().BeEmpty();
        rule.Reason.Should().BeNull();
        rule.Priority.Should().Be(0);
        rule.Effect.Should().Be(PolicyType.Allow);
        rule.Conditions.Should().BeEmpty();
    }

    [Fact]
    public void PolicyRule_InitProperties_SetCorrectly()
    {
        var condition = new PolicyRuleCondition { Field = "method", Operator = "eq", Value = "POST" };
        var rule = new PolicyRule
        {
            Name = "block-post",
            Reason = "Deny all POST requests",
            Priority = 10,
            Effect = PolicyType.Deny,
            Conditions = [condition]
        };

        rule.Name.Should().Be("block-post");
        rule.Reason.Should().Be("Deny all POST requests");
        rule.Priority.Should().Be(10);
        rule.Effect.Should().Be(PolicyType.Deny);
        rule.Conditions.Should().HaveCount(1);
        rule.Conditions[0].Field.Should().Be("method");
    }
}

public sealed class PolicyRuleConditionTests
{
    [Fact]
    public void PolicyRuleCondition_DefaultValues()
    {
        var condition = new PolicyRuleCondition();

        condition.Field.Should().BeEmpty();
        condition.Operator.Should().BeEmpty();
        condition.Value.Should().NotBeNull();
    }

    [Fact]
    public void PolicyRuleCondition_InitProperties_SetCorrectly()
    {
        var condition = new PolicyRuleCondition
        {
            Field = "path",
            Operator = "startsWith",
            Value = "/admin"
        };

        condition.Field.Should().Be("path");
        condition.Operator.Should().Be("startsWith");
        condition.Value.Should().Be("/admin");
    }

    [Fact]
    public void PolicyRuleCondition_Value_CanBeNumeric()
    {
        var condition = new PolicyRuleCondition
        {
            Field = "priority",
            Operator = "gt",
            Value = 5
        };

        condition.Value.Should().Be(5);
    }
}
