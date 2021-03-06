# Kvit

[![Build & Test & Pack](https://github.com/sadedil/kvit/workflows/Build%20&%20Test%20&%20Pack/badge.svg)](https://github.com/sadedil/kvit/actions)
[![NuGet (Kvit)](https://img.shields.io/nuget/v/Kvit.svg)](https://www.nuget.org/packages/Kvit/)

**Kvit** helps you to sync your key and value pairs between *HashiCorp Consul* and file system easily. It's developed as an open source CLI app.

## What "Kvit" means?

**Kvit** name comes from **Consul Key Value** + **Git like usage** = **KV** + **Git** = **kvit**

## How to install?

> .NET 5 runtime is a prerequisite. And can be downloaded from [here](https://dotnet.microsoft.com/download).
> 
**Kvit** developed as a cross platform console app with *.NET 5.* You can easily install as a global cli tool.

```bash
# Install if not installed, update to the last version if already installed
dotnet tool update -g kvit
```

This command pulls the latest Kvit binary from [NuGet](https://www.nuget.org/packages/Kvit/). And add this binary to your path.

## How to use?

After the install, you can easily execute by typing `kvit` to your favorite terminal. When you run without a parameter, you can see the basic usage information.

### TL;DR

- Use `kvit fetch` and download all keys and values to current folder
- Edit this files or add new ones with your favorite text editor
- Optionally use `kvit diff` to see which files are changed
- Optionally use `kvit compare <file>` to see which part of your content is different between file system and consul
- Then use `kvit push` to upload all to your Consul server

> Most kvit commands requires this two parameters: `address` and `token`
> - If you omit `address` then tries to connect ``http://localhost:8500``
> - If you omit `token` then tries to connect without authentication.

### `kvit fetch`

Downloads your key/value pairs from your Consul server, and writes all into the current directory.

```
kvit fetch [--address <address>] [--token <token>]
```

### `kvit push`

Uploads your key/value pairs from current directory to your Consul server.

>Currently kvit not supports deletion of key value pairs (This feature is in our roadmap)

```
kvit push [--address <address>] [--token <token>]
```

### `kvit diff`

Compares keys between Consul and current directory and prints a summary.

- If you add `--all` option, it displays all files in directory, otherwise only display different or missing ones. 
```
kvit diff [--address <address>] [--token <token>] [--all]
```

### `kvit compare`

Compares the content of key between Consul and file system.

- `<file>` is a required parameter. You should use the relative path of your current directory. For example `kvit compare folder1/file1`.  

```
kvit compare [--address <address>] [--token <token>] <file>
```

## How to build & test and run on your computer?

### Requirements
 - *[.NET 5 SDK](https://dotnet.microsoft.com/download)*
 - *Docker* or a real *Consul* server (for testing)

### Running project locally

Simply run on your IDE or type `dotnet run` in `src/Kvit` folder.

### Running tests locally

To run Integration tests, you will need a *Consul* running on port `8900`. Easiest way to do this, using *Docker*.

```bash
docker run -d --name=consul-for-kvit-testing -p 8900:8500 consul
```

After then, you can simply run tests on your IDE or type `dotnet test` in project's root folder. 

## Roadmap

 - [x] A clear README
 - [ ] Take a backup before the `fetch` and `push`  
 - [x] Add diff support before push to see what's different between local folder and remote *Consul* server
 - [ ] Add confirmation messages for commands like "*Do you want to continue?*"
 - [ ] Support the deletion of keys and values from *Consul* when they were deleted from local folder

## How to contribute?

Free to feel to open issues about your questions and PR thoughts.