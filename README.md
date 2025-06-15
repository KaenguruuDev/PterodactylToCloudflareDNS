# PTCF - Pterodactyl to Cloudflare

**PTCF** (Pterodactyl to Cloudflare) is a lightweight console utility that automatically creates DNS records for
Minecraft servers managed through a [Pterodactyl Panel](https://pterodactyl.io/) instance. It allows each server to
be accessed via a custom subdomain, eliminating the need to share raw IP addresses.

## ✨ Features

- ✅ Automatically detects when new Minecraft servers start
- 🌐 Creates or updates DNS A/SRV records via the Cloudflare API
- 📦 Lightweight and easy to deploy

> [!NOTE]
> Please be aware that this project was developed for a very specific setup and it is very likely that you have to
> adjust it to fit to yours.

## Basic Idea

The application runs in two seperate instances, one in Cloudflare mode and one in Pterodactyl mode. They communicate via
a minimal API.

This allows any user who starts a server in the Pterodactyl panel to have a direct connection to that server over
something like "my-server.my-domain.de" instead of using something like "node.my-domain.de:2000".

## Modes

### 1. Cloudflare

Adds/Removes A/AAAA and SRV records for individual servers.

### 2. Pterodactyl

Fetches the configuration of any active servers.

## Setup

For both instances, a `.env` File is needed. They need to have the following configurations:

**Cloudflare**

```
CLOUDFLAREAPIKEY=????
CLOUDFLAREZONEID=????
PTERODACTYLAPIKEY=????
PTERODACTYLAPIURL=????
```

> [!NOTE]
> The "PTERODACTYLAPIURL" key in Cloudflare instances refers to the pterodactyl instance of PTCF. If you are running
> both on the same server, it would therefore be http://localhost:5000 and **not** the same url as the "PTERODACTYLAPIURL"
> key in the pterodactyl instance.


**Pterodactyl**

```
PTERODACTYLAPIKEY=????
PTERODACTYLAPIKEY=????
```

Currently, in **Pterodactyl** mode, the API is always hosted on `localhost:5000`, that will be configurable in a future
version.

## Goals

1. Allow more configuration for the script setup
