# Autofac.Configuration

Configuration support for [Autofac](https://autofac.org).

[![Build status](https://ci.appveyor.com/api/projects/status/u6ujehy60pw4vyi2?svg=true)](https://ci.appveyor.com/project/Autofac/autofac-configuration)

Please file issues and pull requests for this package [in this repository](https://github.com/autofac/Autofac.Configuration/issues) rather than in the Autofac core repo.

- [Documentation](https://autofac.readthedocs.io/en/latest/configuration/xml.html)
- [NuGet](https://www.nuget.org/packages/Autofac.Configuration)
- [Contributing](https://autofac.readthedocs.io/en/latest/contributors.html)
- [Open in Visual Studio Code](https://open.vscode.dev/autofac/Autofac.Configuration)

## Quick Start

The basic steps to getting configuration set up with your application are:

1. Set up your configuration in JSON or XML files that can be read by `Microsoft.Extensions.Configuration`.
   - JSON configuration uses `Microsoft.Extensions.Configuration.Json`
   - XML configuration uses `Microsoft.Extensions.Configuration.Xml`
2. Build the configuration using the `Microsoft.Extensions.Configuration.ConfigurationBuilder`.
3. Create a new `Autofac.Configuration.ConfigurationModule` and pass the built `Microsoft.Extensions.Configuration.IConfiguration` into it.
4. Register the `Autofac.Configuration.ConfigurationModule` with your container.

A configuration file with some simple registrations looks like this:

```json
{
  "defaultAssembly": "Autofac.Example.Calculator",
  "components": [{
    "type": "Autofac.Example.Calculator.Addition.Add, Autofac.Example.Calculator.Addition",
    "services": [{
      "type": "Autofac.Example.Calculator.Api.IOperation"
    }],
    "injectProperties": true
  }, {
    "type": "Autofac.Example.Calculator.Division.Divide, Autofac.Example.Calculator.Division",
    "services": [{
      "type": "Autofac.Example.Calculator.Api.IOperation"
    }],
    "parameters": {
      "places": 4
    }
  }]
}
```

JSON is cleaner and easier to read, but if you prefer XML, the same configuration looks like this:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<autofac defaultAssembly="Autofac.Example.Calculator">
    <components name="0">
        <type>Autofac.Example.Calculator.Addition.Add, Autofac.Example.Calculator.Addition</type>
        <services name="0" type="Autofac.Example.Calculator.Api.IOperation" />
        <injectProperties>true</injectProperties>
    </components>
    <components name="1">
        <type>Autofac.Example.Calculator.Division.Divide, Autofac.Example.Calculator.Division</type>
        <services name="0" type="Autofac.Example.Calculator.Api.IOperation" />
        <injectProperties>true</injectProperties>
        <parameters>
            <places>4</places>
        </parameters>
    </components>
</autofac>
```

*Note the ordinal "naming" of components and services in XML - this is due to the way Microsoft.Extensions.Configuration handles ordinal collections (arrays).*

Build up your configuration and register it with the Autofac `ContainerBuilder` like this:

```c#
// Add the configuration to the ConfigurationBuilder.
var config = new ConfigurationBuilder();
// config.AddJsonFile comes from Microsoft.Extensions.Configuration.Json
// config.AddXmlFile comes from Microsoft.Extensions.Configuration.Xml
config.AddJsonFile("autofac.json");

// Register the ConfigurationModule with Autofac.
var module = new ConfigurationModule(config.Build());
var builder = new ContainerBuilder();
builder.RegisterModule(module);
```

Check out the [Autofac configuration documentation](https://autofac.readthedocs.io/en/latest/configuration/xml.html) for more information.

## Get Help

**Need help with Autofac?** We have [a documentation site](https://autofac.readthedocs.io/) as well as [API documentation](https://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).
