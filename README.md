# PTCF - Pterodactyl to Cloudflare

**PTCF** (Pterodactyl to Cloudflare) is a lightweight console utility that automatically creates DNS records for
Minecraft servers managed through a [Pterodactyl Panel](https://pterodactyl.io/) instance. It allows each server to
be accessed via a custom subdomain, eliminating the need to share raw IP addresses.

## ✨ Features

- ✅ Automatically detects when new Minecraft servers start
- 🌐 Creates or updates DNS A/SRV records via the Cloudflare API
- � **Email notifications for DNS operations (create, update, delete, errors)**
- �📦 Lightweight and easy to deploy

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
PTERODACTYL_CLIENT_API_KEY=????
PTERODACTYL_APPLICATION_API_KEY=????
PTERODACTYLAPIURL=????
```

**Email Notifications (Optional)**

To enable email notifications for DNS operations, add the following to your configuration file:

```
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
FROM_ADDRESS=your-email@gmail.com
EMAIL_SUBSCRIBERS=admin1@example.com,admin2@example.com
COMPANY_NAME=Your Company Name
BRAND_COLOR=#3b82f6
COMPANY_LOGO_URL=https://example.com/logo.png
```

Email notifications will be sent for:
- ✅ New DNS records created
- 🔄 DNS records updated
- 🗑️ DNS records deleted
- ⚠️ DNS operation failures (creation, update, deletion)

> [!TIP]
> For Gmail users, you need to create an [App Password](https://support.google.com/accounts/answer/185833) instead of using your regular password.
> Multiple email subscribers can be specified by separating them with commas.

**Testing Email Configuration**

To test your email configuration and preview all email templates, run the program with the `test-email` or `email-test` argument:

```bash
dotnet run config.txt test-email
```

This will send test emails for all templates:
- New DNS records created
- DNS records updated
- DNS records deleted
- A/SRV record creation failures
- A/SRV record update failures
- A/SRV record deletion failures

The program will exit after sending all test emails. Check your inbox to verify the templates look correct.

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
