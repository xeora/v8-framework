# Xeora Web Development Framework

Xeora is a cross-platform .NET web development framework with its own built-in HTTP server, template rendering engine, and domain-based application architecture. It provides an alternative to ASP.NET MVC for developers who want full control over their web stack while staying in the .NET ecosystem.

v8 is a ground-up rewrite with significant improvements in rendering speed, modularity, and cross-platform support.

## Features

- **Self-hosted HTTP server** with configurable connection pooling and bandwidth management
- **SSL/TLS 1.2** support with PKCS#12 certificate authentication
- **WebSocket** support for real-time communication
- **Custom template engine** with a directive-based syntax for server-side rendering
- **Domain-based architecture** with nested domain and add-on support
- **Inline C# execution** directly within templates via statement tags
- **Session management** with built-in or distributed (DSS) session storage
- **Worker pool** with adaptive scheduling and priority-based task execution
- **Task scheduler** for background jobs with absolute and relative time triggers
- **Domain compilation** with optional password-based encryption for deployment
- **HTTP response compression** enabled by default
- **Performance analysis** with configurable threshold logging
- **Cross-platform** - runs on Linux, macOS, and Windows. Docker-ready.

## Prerequisites

- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) or later

## Getting Started

### Install the CLI

```bash
dotnet tool install -g Xeora.CLI
```

### Create a New Project

```bash
xeora create -x ./my-app -d Main -c en-US -n English
```

This generates the project structure with a default domain, language files, templates, and a `xeora.settings.json` configuration file.

### Run the Development Server

```bash
cd my-app
xeora run
```

The server starts on `http://localhost:3381` by default. Port and address are configurable in `xeora.settings.json`.

## CLI Reference

| Command | Description |
|---|---|
| `xeora create` | Scaffold a new Xeora project with domain structure and configuration |
| `xeora run` | Start the web server for development or production |
| `xeora compile` | Compile domains into binary format with optional encryption |
| `xeora publish` | Package the project for deployment |
| `xeora extract` | Extract a compiled domain back to source |
| `xeora framework` | Download or update the Xeora framework engine |
| `xeora dss` | Start the Distributed Session Service |

Run `xeora <command> --help` for detailed options on each command.

### Typical Workflow

```bash
# Create project
xeora create -x ./my-app -d MyDomain

# Develop with live server
xeora run

# Compile for release (with encryption)
xeora compile -d MyDomain -p <password> -o ./release ./my-app

# Publish to deployment target
xeora publish -o ./dist -c ./my-app
```

## Configuration

Application settings live in `xeora.settings.json` at the project root. Key configuration areas:

**Service**
- `address` - Bind address (default: `127.0.0.1`)
- `port` - Listen port (default: `3381`)
- `ssl` - Enable SSL/TLS with certificate path and password
- `timeout` - Read/write timeouts in milliseconds
- `parallelism` - Max connections and worker magnitude
- `logging` / `loggingFormat` / `loggingLevel` - Logging control (plain or JSON, levels: trace/debug/info/warn/error)

**Application**
- `physicalRoot` / `virtualRoot` / `applicationRoot` - Path configuration
- `defaultDomain` - Entry domain ID
- `debugging` - Enable debug mode
- `compression` - HTTP response compression
- `performanceAnalysis` - Request timing with threshold
- `bandwidthLimit` - Bandwidth throttling (0 = unlimited)
- `customMime` - Additional MIME type mappings
- `bannedFiles` - Regex patterns to restrict file access

**Session**
- `cookieKey` - Session cookie name (default: `xcsid`)
- `timeout` - Session expiration in minutes (default: `20`)

## Template Syntax Overview

Xeora uses a `$` delimited tag syntax for server-side rendering. A brief summary:

```
$T:TemplateID$                    Template include
$L:TranslationID$                 Language translation
$C:ControlID$                     Control rendering
$F:Lib?Class.Method,args$         Server-side function call
$S:ID:{ C# code }:ID$            Inline C# statement
$H:ID:{ content }:ID$            Request block
$PC:{ content }:PC$              Partial page cache
$MB:{ content }:MB$              Message block (error/success/warning)
```

**Variable operators:**

| Prefix | Source |
|---|---|
| `$Key$` | Variable pool |
| `$^Key$` | Query string |
| `$~Key$` | Form POST |
| `$-Key$` | Session |
| `$+Key$` | Cookie |
| `$=Key$` | Literal value |
| `$#Key$` | Data block variable |
| `$*Key$` | Search all sources |

Controls are defined in `Controls.xml` with types including `ConditionalStatement`, `DataList`, `VariableBlock`, `Button`, `Textbox`, `Password`, `LinkButton`, and more.

See [QuickGuide.txt](QuickGuide.txt) for the full syntax reference or visit the [documentation](http://www.xeora.org/documentation).

## Architecture

The framework is organized into focused modules:

```
Xeora.CLI                      CLI tool (create, run, compile, publish)
Xeora.Web.Service              Self-hosted TCP/HTTP server with SSL
Xeora.Web.Handler              HTTP request pipeline
Xeora.Web                      Template rendering engine and directives
Xeora.Web.Manager              Assembly loading and bind execution
Xeora.Web.Configuration        All configuration models
Xeora.Web.Basics               Public API interfaces and contracts (NuGet)
Xeora.Web.Exceptions           Framework exception hierarchy
Xeora.Web.Service.Context      HTTP request/response and WebSocket
Xeora.Web.Service.Net          Network stream handling
Xeora.Web.Service.Session      Session state management
Xeora.Web.Service.Dss          Distributed Session Service (IPC)
Xeora.Web.Service.Workers      Worker pool and connection management
Xeora.Web.Service.TaskScheduler Background task scheduling
Xeora.Web.Service.VariablePool Global variable storage
Xeora.Web.Service.Application  Application container management
```

`Xeora.Web.Basics` is the public contract package published to NuGet, multi-targeting net6.0 through net9.0.

## Building from Source

```bash
cd src
dotnet restore Framework.sln
dotnet build Framework.sln
```

Platform-specific release builds output to `src/build/<platform>/`:

```bash
dotnet build Framework.sln -c Release -p:Platform=arm64
dotnet build Framework.sln -c Release -p:Platform=x64
```

## Contributing

Contributions are welcome! Please use the [issue tracker](https://github.com/xeora/v8/issues) for bug reports and feature proposals.

## License

[MIT](LICENSE) - Copyright (c) 2003 Tuna Celik
