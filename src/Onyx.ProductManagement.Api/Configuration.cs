﻿using System.Diagnostics.CodeAnalysis;

namespace Onyx.ProductManagement.Api;

[ExcludeFromCodeCoverage]
public class JwtSettings
{
    public const string SectionName = "Jwt";
        
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 60;
}