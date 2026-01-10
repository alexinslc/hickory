using FluentAssertions;
using Hickory.Api.Common;
using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Tests.Common;

/// <summary>
/// Tests to ensure AuthorizationRoles constants match UserRole enum values
/// </summary>
public class AuthorizationRolesTests
{
    [Fact]
    public void EndUser_ShouldMatchEnumValue()
    {
        // Arrange & Act
        var expected = nameof(UserRole.EndUser);
        
        // Assert
        AuthorizationRoles.EndUser.Should().Be(expected);
        AuthorizationRoles.EndUser.Should().Be("EndUser");
    }

    [Fact]
    public void Agent_ShouldMatchEnumValue()
    {
        // Arrange & Act
        var expected = nameof(UserRole.Agent);
        
        // Assert
        AuthorizationRoles.Agent.Should().Be(expected);
        AuthorizationRoles.Agent.Should().Be("Agent");
    }

    [Fact]
    public void Administrator_ShouldMatchEnumValue()
    {
        // Arrange & Act
        var expected = nameof(UserRole.Administrator);
        
        // Assert
        AuthorizationRoles.Administrator.Should().Be(expected);
        AuthorizationRoles.Administrator.Should().Be("Administrator");
    }

    [Fact]
    public void AgentOrAdministrator_ShouldContainBothRoles()
    {
        // Arrange & Act
        var expected = $"{nameof(UserRole.Agent)},{nameof(UserRole.Administrator)}";
        
        // Assert
        AuthorizationRoles.AgentOrAdministrator.Should().Be(expected);
        AuthorizationRoles.AgentOrAdministrator.Should().Be("Agent,Administrator");
    }

    [Fact]
    public void AllRoleConstants_ShouldNotBeNullOrEmpty()
    {
        // Assert
        AuthorizationRoles.EndUser.Should().NotBeNullOrWhiteSpace();
        AuthorizationRoles.Agent.Should().NotBeNullOrWhiteSpace();
        AuthorizationRoles.Administrator.Should().NotBeNullOrWhiteSpace();
        AuthorizationRoles.AgentOrAdministrator.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("EndUser")]
    [InlineData("Agent")]
    [InlineData("Administrator")]
    public void RoleConstants_ShouldMatchToStringOfEnumValue(string roleName)
    {
        // Arrange
        var enumValue = Enum.Parse<UserRole>(roleName);
        
        // Act
        var enumString = enumValue.ToString();
        
        // Assert - Get the constant value by reflection
        var fieldInfo = typeof(AuthorizationRoles).GetField(roleName);
        fieldInfo.Should().NotBeNull($"AuthorizationRoles.{roleName} should exist");
        
        var constantValue = fieldInfo!.GetValue(null) as string;
        constantValue.Should().Be(enumString, $"AuthorizationRoles.{roleName} should match UserRole.{roleName}.ToString()");
    }
}
