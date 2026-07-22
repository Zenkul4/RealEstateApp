# Configuración local - Seguridad MVC y WebAPI

Los proyectos WebApp y WebAPI comparten el identificador de User Secrets
`RealEstateApp-2026-Dev2`. Ninguna contraseña ni clave JWT debe guardarse en
`appsettings.json`.

> Si una contraseña SMTP o clave JWT fue publicada alguna vez en Git, debe
> revocarse y reemplazarse. Eliminarla del último commit no la elimina del
> historial del repositorio.

## Configurar secretos

Ejecutar desde la raíz de la solución y reemplazar los valores de ejemplo:

```powershell
dotnet user-secrets set "JWTSettings:Key" "UNA-CLAVE-ALEATORIA-DE-AL-MENOS-32-CARACTERES" --project .\RealEstateApp.Presentation.WebApi
dotnet user-secrets set "SeedUsers:Admin:Password" "CONTRASENA-SEGURA" --project .\RealEstateApp.Presentation.WebApi
dotnet user-secrets set "SeedUsers:Developer:Password" "CONTRASENA-SEGURA" --project .\RealEstateApp.Presentation.WebApi
dotnet user-secrets set "SeedUsers:Client:Password" "CONTRASENA-SEGURA" --project .\RealEstateApp.Presentation.WebApi
dotnet user-secrets set "SeedUsers:Agent:Password" "CONTRASENA-SEGURA" --project .\RealEstateApp.Presentation.WebApi
dotnet user-secrets set "MailSettings:UserName" "USUARIO-SMTP" --project .\RealEstateApp.Presentation.WebApp
dotnet user-secrets set "MailSettings:Password" "CONTRASENA-SMTP" --project .\RealEstateApp.Presentation.WebApp
dotnet user-secrets set "MailSettings:FromEmail" "CORREO-REMITENTE" --project .\RealEstateApp.Presentation.WebApp
```

Para un servidor SMTP distinto al valor de ejemplo también se pueden configurar:

```powershell
dotnet user-secrets set "MailSettings:Host" "smtp.proveedor.com" --project .\RealEstateApp.Presentation.WebApp
dotnet user-secrets set "MailSettings:Port" "587" --project .\RealEstateApp.Presentation.WebApp
dotnet user-secrets set "MailSettings:UseSsl" "false" --project .\RealEstateApp.Presentation.WebApp
```

Para Gmail se debe utilizar una contraseña de aplicación, no la contraseña
normal de la cuenta. Con el puerto `587`, `UseSsl` debe ser `false` porque
MailKit negocia STARTTLS. `FromEmail` debe coincidir con la cuenta autenticada
o con un alias de envío autorizado por Gmail.

El servicio no simula envíos: una configuración incompleta o un rechazo SMTP
produce un error. Durante el registro de clientes, la aplicación revierte el
usuario y elimina la foto si el correo de activación no puede enviarse.

En producción, usar variables de entorno con doble guion bajo, por ejemplo:
`JWTSettings__Key` y `MailSettings__Password`.

## Rutas principales

- WebApp Login: `/Account/Login`
- WebApp Registro: `/Account/Register`
- Swagger UI: `/swagger`
- Documento OpenAPI: `/swagger/v1/swagger.json`
- Login API: `POST /api/Account/authenticate`
- Registrar desarrollador: `POST /api/Account/register-developer`
- Registrar administrador: `POST /api/Account/register-admin`
- Propiedades: `/api/properties`
- Agentes: `/api/agents`
- Tipos de propiedad: `/api/propertytypes`
- Tipos de venta: `/api/saletypes`
- Mejoras: `/api/improvements`

Las rutas anteriores `/api/account/login`, `/api/account/developers`,
`/api/account/administrators`, `/api/property-types` y `/api/sale-types` se
mantienen temporalmente como alias de compatibilidad.

Los endpoints de consulta aceptan `Administrador` y `Desarrollador`. Las
operaciones de creación, edición, eliminación y cambio de estado requieren
`Administrador`.
