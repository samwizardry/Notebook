## Docker

Полезные cli команды, обучающие материалы, напоминания.

### CLI Cheat Sheet

[Cheet Sheat по докеру](docker_cheatsheet.pdf)

#### Дополнительные команды которые не попали в документ

Пример запуска asp.net приложения

```
docker run --rm [-d] -p 80:8080 --name app app:latest
```

Открыть bash терминал внутри контейнера

```
docker exec -it <имя контейнера> /bin/bash
```

Сокращение для интерактивного режима и запуска на заднем фоне

```
docker run -itd
```

Clear build cache

```
docker builder prune -af
```

Удалить контейнер, включая сеть, (volume продолжить существовать)

```
docker compose down
```

### References

- [Docker Tutorial for Beginners [FULL COURSE in 3 Hours]](https://www.youtube.com/watch?v=3c-iBn73dDE&ab_channel=TechWorldwithNana)
- [Docker networking is CRAZY!! (you NEED to learn it)](https://www.youtube.com/watch?v=bKFMS5C4CG0&ab_channel=NetworkChuck)

### Links
- [docker.docs](https://docs.docker.com/guides/)