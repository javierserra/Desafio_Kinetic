# 📁 Desafio Kinetic - Procesamiento de Archivos TXT en Lotes

## 📌 Enfoque y Decisiones de Diseño

Este proyecto fue diseñado como una API RESTful construida en **ASP.NET Core 8**, con **Hangfire** para la ejecución en segundo plano de procesos batch. El sistema procesa archivos `.txt` en carpetas, extrayendo estadísticas y generando resúmenes automáticos de su contenido.

### Decisiones clave:

- ✅ **Hangfire** permite manejar trabajos en segundo plano de forma escalable y desacoplada.
- ✅ Se usó **ProcessStateStore** para persistencia local y resiliencia simple mediante archivos JSON.
- ✅ El procesamiento es **independiente** por carpeta, lo que facilita reinicios seguros.
- ✅ La lógica del negocio se encuentra en `FolderProcessor`, con separación clara de responsabilidades.

---

## 🛠️ Instalación y Uso

### ✅ Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker](https://www.docker.com/) (opcional)
- Hangfire Dashboard (opcional)


### 🚀 Ejecución Docker

```bash
# Clonar el proyecto
git clone https://github.com/javierserra/Desafio_Kinetic.git
cd Desafio_Kinetic

# Ejecutar
docker compose -f docker-compose.yml up -d --build 
```


-  El API estará disponible en: `http://localhost:8080`
-  La documentación Swagger UI estará disponible en: `http://localhost:8080/swagger`
-  El monitoreo del servicio Hangfire estará disponible en: `http://localhost:8080/hangfire`

### 🧪 Prueba del procesamiento

1. Crea una estructura con subcarpetas en `/app/input` y coloca archivos `.txt` en ellas. El repo original cuanta con carpetas y archivos de prueba en la ruta indicada.
2. Ejecuta un `POST` a `http://localhost:8080/api/processes` con:

```json
{
  "rootPath": "/app/input"
}
```

3. Consulta el estado y resultado:

- `GET /api/processes/{id}/status`
- `GET /api/processes/{id}/result`

---

## 🔁 Estrategia de Resiliencia

### 🔄 ¿Qué sucede si la aplicación se reinicia?

- El estado de cada proceso se guarda en disco (`state.json`), por lo que al reiniciar, los procesos ya ejecutados no se pierden.
- Se puede consultar el estado de todos los procesos aún después del reinicio.

### ❌ ¿Y si un archivo está corrupto?

- El sistema atrapa excepciones durante el procesamiento individual.
- El archivo es marcado como `"FAILED"` y el resto del lote continúa.
- El estado del proceso puede quedar como `"FAILED"` si al menos un archivo falla.
- Aún es posible agregar gestión de diversos errores. Se considera apropiado este nivel para el MVP
---

## 🧱 Arquitectura Futura. 

### 1️⃣ Escalabilidad

Para procesar millones de documentos al día desde múltiples fuentes:

**Cambio 1: Desacoplar procesamiento por colas**
- Reemplazar `Hangfire` por colas distribuidas como **AWS SQS**, **RabbitMQ** o **Kafka** para distribuir el trabajo en múltiples workers.

**Cambio 2: Almacenamiento en nube**
- Migrar `ProcessStateStore` y `GenDocs` a almacenamiento persistente o temporal como **DynamoDB**, **Redis** o **MongoDB Atlas**.

**Cambio 3: Workers distribuidos y autoscaling**
- Generar COntenedores y escalar los workers (`FolderProcessor`) horizontalmente en un orquestador como Kubernetes.

---

### 2️⃣ Infraestructura Recomendada. Menciono los servicios de AWS, pero encontrarán equivalentes en otros Clouds.

| Recurso | Justificación |
|--------|----------------|
| **AWS Lambda / ECS Fargate** | Serverless o contenedores autogestionados para ejecutar los workers por lote |
| **S3** | Para almacenar archivos de entrada y salidas JSON |
| **DynamoDB o MongoDB Atlas** | Para almacenar estados y resultados de procesos |
| **SQS** | Para orquestar eventos de carpetas nuevas o archivos nuevos |
| **CloudWatch / OpenTelemetry / Loki-Grafana** | Monitoreo y trazabilidad de procesos |

---

## 🧪 Instrucciones para pruebas

### ✅ Pruebas manuales

- Ejecutar el servicio sobre un contendor.
- Cargar archivos `.txt` en carpetas en la ruta que defina el mapeo.
- Usar herramientas como Postman o curl para probar los endpoints:
  - `POST /api/processes`
  - `GET /api/processes/{id}/status`
  - `GET /api/processes/{id}/result`

---

## 📚 Documentación OpenAPI

El proyecto está autodocumentado con Swagger. Accede a la documentación navegando a:

```
http://localhost:8080/swagger
```

Incluye:

- Descripción de cada endpoint.
- Ejemplos de entrada y salida.
- Códigos de estado esperados.


## 📚 Utilización de AI

No tengo mucha experiencia (al menos en los últmos años) en el desarrollo sobre .NET, por lo que gran parte del código fué etructurado desde las siguientes consulta a ChatGPT y GeminiAI segùn el caso. Los test por ejemplo son completamente generados por AI:

```
PROMPT 1:
En .NET tengo que ejecutar y administrar un proceso asíncrono que será iniciado desde un endpoint, y desde otros pausado, verificado status y resolución. Que librerías me recomiendan para administrar este tipo proceso?

De las 4 posibilidades ofrecidas se elige Hangfire que está diseñada específicamente para tratamiento en Batch y presenta un tablero de monitoreo propio como plus a la solución.

Como no conozco la solución, pido a la  AI un ejemplo de cómo implementar esta librería en una API .NET con una arquitectura de capas separando servicios de controllers y models 
PROMPT 2:
Por favor, dame un ejemplo de cómo implementar Hangfire en una API con un namespace services donde residirán cada una de las acciones sobre una estructura de archivos inferida de un path a procesar, iniciando un job por cada subcarpeta del path.

El resultado obtenido se usa como base para iniciar el desarrollo y subo un primer commit funcional a github para ir registrando el avance.
Para no avanzar más sin agregar logging, consulto sobre el ejemplo:

PROMPT 3:
Cómo aplicar un login estructurado a este código?
Adapto la respuesta del chat y lo subo al repo en un segundo commit. 


PROMPT 4:
Hola, quisiera que me ayudes a construir una lista predefinida en castellano de stop_words comunes como "el", "la", "que", "y", "a", etc.


PROMPT 5:
Hola, quisiera que me orientaras en cómo generar una persistencia de un diccionario JSON que pueda ser utilizado por distintos endpoints de mi API c# para mantener un proceso stateful ● PENDING: Proceso creado pero no iniciado. ● RUNNING: Procesamiento en curso. ● PAUSED: Proceso pausado temporalmente. ● COMPLETED: Proceso finalizado con éxito. ● FAILED: Proceso terminado con errores. ● STOPPED: Proceso detenido manualmente.
El resultado obtenido se usa como base para desarrollar la persistencia de los estados y los resultados en procesos paralelos y se realiza commit

PROMPT 6:
Hola, tengo un sistema que hace un procesamiento por lotes de archivos con el siguiente requisito funcional Extraer estadísticas básicas: conteo de palabras, líneas y caracteres. Identificar las 10 palabras más frecuentes (excluyendo "stop words" comunes como "el","la", "que", "y", "a", etc. Puedes usar una lista predefinida).Generar un resumen de contenido simple con saltos de línea (ej. tomando las primeras 3-4 oraciones del documento). Me gustaría que generes una carpeta con 3 lotes y 10 archivos en cada uno en un archivo zip que pueda descargar. cada archivo debe tener contenido real en castellano variado (mínimo 500 palabras cada uno) para demostrar la funcionalidad del sistema.

PROMPT 7:
hola, tengo que leer estos archivos en c# y generar un resumen de su contenido. ¿cómo podría generar este contenido resumen?

```
