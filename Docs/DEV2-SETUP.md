# Configuración local - Seguridad MVC y WebAPI

Los proyectos WebApp y WebAPI comparten el identificador de User Secrets
`RealEstateApp-2026-Dev2`. Ninguna contraseña ni clave JWT debe guardarse en
`appsettings.json`.

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
```

Para un servidor SMTP distinto al valor de ejemplo también se pueden configurar:

```powershell
dotnet user-secrets set "MailSettings:Host" "smtp.proveedor.com" --project .\RealEstateApp.Presentation.WebApp
dotnet user-secrets set "MailSettings:Port" "587" --project .\RealEstateApp.Presentation.WebApp
dotnet user-secrets set "MailSettings:FromEmail" "no-reply@dominio.com" --project .\RealEstateApp.Presentation.WebApp
dotnet user-secrets set "MailSettings:UseSsl" "false" --project .\RealEstateApp.Presentation.WebApp
```

En producción, usar variables de entorno con doble guion bajo, por ejemplo:
`JWTSettings__Key` y `MailSettings__Password`.

## Rutas principales

- WebApp Login: `/Account/Login`
- WebApp Registro: `/Account/Register`
- Documento OpenAPI: `/openapi/v1.json`
- Login API: `POST /api/account/login`
- Propiedades: `/api/properties`
- Agentes: `/api/agents`
- Tipos de propiedad: `/api/property-types`
- Tipos de venta: `/api/sale-types`
- Mejoras: `/api/improvements`

Los endpoints de consulta aceptan `Administrador` y `Desarrollador`. Las
operaciones de creación, edición, eliminación y cambio de estado requieren
`Administrador`.
