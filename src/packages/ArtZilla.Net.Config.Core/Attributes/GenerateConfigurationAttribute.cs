using System;

namespace ArtZilla.Net.Config;

/// used by ArtZilla.Net.Config.Generators package to mark types to generate
[AttributeUsage(AttributeTargets.Interface)]
public class GenerateConfigurationAttribute : Attribute { }