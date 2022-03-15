<h1>RabbitMQ tests</h1>

<h2>Docker start</h2>

```text
docker run -d --hostname my-rabbit --name some-rabbit -p 8080:15672 -e RABBITMQ_DEFAULT_USER=admin -e RABBITMQ_DEFAULT_PASS=89831143406 rabbitmq:3-management
```

<h2>Managment</h2>

```text
http://localhost:8080/#/
```