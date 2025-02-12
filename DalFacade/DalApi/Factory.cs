﻿namespace DalApi;
using System.Reflection;
using static DalApi.DalConfig;

/// <summary>
/// Here the classes instances of the database is created.
/// </summary>
public static class Factory
{
    public static IDal? Get()
    {
        string dalType = s_dalName
            ?? throw new DalConfigException($"DAL name is not extracted from the configuration");
        string dal = s_class
           ?? throw new DalConfigException($"Class for {dalType} is not found in packages list");
        string dalNamespace = s_namespace
            ?? throw new DalConfigException($"Namespace for {dalType} is not found in packages list");
        try
        {
            Assembly.Load(dal ?? throw new DalConfigException($"Package {dal} is null"));
        }
        catch (Exception)
        {
            throw new DalConfigException("Failed to load {dal}.dll package");
        }

        Type? type = Type.GetType($"{dalNamespace}.{dal}, {dal}")
            ?? throw new DalConfigException($"Class Dal.{dal} was not found in {dal}.dll");

        return type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?
                   .GetValue(null) as IDal
            ?? throw new DalConfigException($"Class {dal} is not singleton or Instance property not found");
    }
}



