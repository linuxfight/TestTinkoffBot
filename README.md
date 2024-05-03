# Tinkoff kassa testing bot

This is a telegram bot for testing link generation in Tiknoff Kassa payments api.

Example ```compose.yml```:
```yaml
services:
  telegrambot:
    image: ghcr.io/linuxfight/testtinkoffbot:master
    restart: unless-stopped
    environment:
      - BOT_TOKEN=YOURTELEGRAMBOTTOKENHERE
    volumes:
      - telegrambotdb:/app/Database
volumes:
  telegrambotdb:
```

To run you simply:
```
docker compose up -d
```
