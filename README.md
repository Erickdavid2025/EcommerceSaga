Requisitos:
Ejecutamos el rabbit en un contenedor en docker, para no tener que descargar la aplicación.

1-docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
usuario y clave : guest , guest
