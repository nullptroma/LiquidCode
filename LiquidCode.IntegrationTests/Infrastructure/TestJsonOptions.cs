using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.IntegrationTests.Infrastructure;

internal static class TestJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
