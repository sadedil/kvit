# Kvit

[![Build & Test & Pack](https://github.com/sadedil/kvit/workflows/Build%20&%20Test%20&%20Pack/badge.svg)](https://github.com/sadedil/kvit/actions)
[![NuGet (Kvit)](https://img.shields.io/nuget/v/Kvit.svg)](https://www.nuget.org/packages/Kvit/)

**Kvit** helps you to sync your key and value pairs between HashiCorp Consul and file system easily. It's developed as an open source CLI app.

## What "Kvit" means?

**Kvit** name comes from **Consul Key Value** + **Git like usage** = **KV** + **Git** = **kvit**

## How to install?

> .NET 5 runtime is a prerequisite. And can be downloaded from [here](https://dotnet.microsoft.com/download).
> 
**Kvit** developed as a cross platform console app with .NET 5. You can easily install as a global cli tool.

```bash
dotnet tool install -g kvit
```

This command pulls the latest Kvit binary from [NuGet](https://www.nuget.org/packages/Kvit/). And add this binary to your path.

## How to use?

After the install, you can easily execute by typing `kvit` to your favorite terminal. When you run without a parameter, you can see the basic usage information.

### TL;DR

- Use `kvit fetch` and download all key values to current folder
- Edit this files or add new ones with your favorite text editor
- Then use `kvit push` to upload all to your Consul server

### `kvit fetch`

Downloads your key/value pairs from your Consul server, and writes all into the current directory.

- If you omit `address` then tries to connect ``http://localhost:8500``
- If you omit `token` then tries to connect without authentication.

```
kvit fetch [--address <address>] [--token <token>]
```

### `kvit push`

Uploads your key/value pairs from current directory to your Consul server.

>Currently kvit not supports deletion of key value pairs (This feature is in our roadmap)

- If you omit `address` then tries to connect ``http://localhost:8500``
- If you omit `token` then tries to connect without authentication.

```
kvit push [--address <address>] [--token <token>]
```

## Roadmap

 - [ ] Take a backup before the `fetch` and `push`  
 - [ ] Add diff support before push to see what's different between local folder and remote Consul server
 - [ ] Add confirmation messages for commands like "*Do you want to continue?*"
 - [ ] Support the deletion of keys and values from Consul when they were deleted from local folder

## How to contribute?

Free to feel to open issues about your questions and PR thoughts.