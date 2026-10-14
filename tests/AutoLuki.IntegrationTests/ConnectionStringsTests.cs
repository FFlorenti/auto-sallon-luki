using AutoLuki.Infrastructure.Persistence;
using Npgsql;

namespace AutoLuki.IntegrationTests;

public class ConnectionStringsTests
{
    [Fact]
    public void Key_value_connection_string_is_left_unchanged()
    {
        const string value = "Host=localhost;Port=5432;Database=autoluki;Username=app;Password=secret";

        Assert.Equal(value, ConnectionStrings.Normalize(value));
    }

    [Theory]
    [InlineData("postgresql://app:secret@db.example.com/autoluki?sslmode=require&channel_binding=require")]
    [InlineData("postgres://app:secret@db.example.com/autoluki")]
    public void Url_style_connection_string_is_converted(string url)
    {
        var result = new NpgsqlConnectionStringBuilder(ConnectionStrings.Normalize(url));

        Assert.Equal("db.example.com", result.Host);
        Assert.Equal("autoluki", result.Database);
        Assert.Equal("app", result.Username);
        Assert.Equal("secret", result.Password);
        Assert.Equal(SslMode.Require, result.SslMode);
    }

    [Fact]
    public void Port_and_encoded_password_are_kept()
    {
        var result = new NpgsqlConnectionStringBuilder(
            ConnectionStrings.Normalize("postgresql://app:p%40ss%3Aword@db.example.com:6543/autoluki?sslmode=verify-full"));

        Assert.Equal(6543, result.Port);
        Assert.Equal("p@ss:word", result.Password);
        Assert.Equal(SslMode.VerifyFull, result.SslMode);
    }
}
