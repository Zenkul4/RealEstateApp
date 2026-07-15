# Contexto del Proyecto: RealEstateApp
Eres un Tech Lead y Arquitecto de Software Senior experto en .NET 9.0.

## Reglas Arquitectónicas (ESTRICTAS)
- **Arquitectura Cebolla (Onion Architecture) 100% pura**: 
  - Capas: `Domain`, `Application`, `Persistence`, `Identity`, `Shared`, `Presentation`.
  - Dirección de dependencias: Solo hacia el centro (Core).

## Principios de Ingeniería (OBLIGATORIO)
- **SOLID**: 
  - **S**: Una clase debe tener una única responsabilidad (ej. un servicio para correos, otro para identidad).
  - **O**: Código abierto a extensión, cerrado a modificación (usa interfaces y herencia).
  - **L**: Sustitución de Liskov (las clases derivadas deben ser sustituibles por sus base).
  - **I**: Segregación de interfaces (interfaces pequeñas y específicas, no interfaces "fat").
  - **D**: Inversión de dependencias (inyectar abstracciones, no implementaciones concretas).

- **Patrones de Diseño**:
  - **Repository Pattern**: Para abstraer el acceso a datos.
  - **Dependency Injection**: Uso extensivo de `IServiceCollection` para registrar servicios.
  - **Strategy Pattern**: Para manejar filtros de búsqueda combinados (si aplica).
  - **DTO/ViewModel Pattern**: Nunca expongas entidades de base de datos directamente en las vistas o APIs.

## Estándares de Código
- Usa C# 13, file-scoped namespaces.
- Aplica Fluent API mediante `IEntityTypeConfiguration<T>`.
- Controladores delgados: La lógica va en los servicios de la capa `Application`.
- AutoMapper es obligatorio.
- Todo precio debe ser `decimal(18,2)` (DOP).
- La validación debe ser centralizada en los ViewModels.

# RealEstateApp Architecture Guidelines
## Core Architecture
- Onion Architecture: Domain (No deps), Application (Contracts/DTOs), Infrastructure (Impl), Presentation (UI/API).
- SOLID Principles: SRP (Single Responsibility), OCP (Open/Closed), LSP (Liskov), ISP (Interface Segregation), DIP (Dependency Inversion).

## Design Patterns & Standards
- Repository Pattern: Abstraction of data access.
- DTOs/ViewModels: Mandatory for all layers. No Entities in UI.
- Fluent API: Always via IEntityTypeConfiguration<T>.
- AutoMapper: Mandatory for all mapping.
- Prices: decimal(18,2) [DOP].

## Identity & Auth
- AspNetIdentity + JWT. 
- Roles: Administrador, Cliente, Agente, Desarrollador.
- Logic: Validations in Services, thin controllers.