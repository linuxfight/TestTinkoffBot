# Tinkoff kassa testing bot

This is a telegram bot for testing link generation in Tiknoff Kassa payments api.

To run you simply:
```
docker pull ghcr.io/linuxfight/testtinkoffbot:master
docker run --restart unless-stopped -e BOT_TOKEN=YOURTELEGRAMBOTTOKENHERE -d ghcr.io/linuxfight/testtinkoffbot:master
```
