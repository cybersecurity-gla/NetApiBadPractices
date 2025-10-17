# API Base con Malas Prácticas - Ejemplo Educativo

⚠️ **ADVERTENCIA**: Este proyecto contiene deliberadamente MALAS PRÁCTICAS de desarrollo. No usar en producción.

## Malas Prácticas Implementadas

### 🔴 Estructura y Organización
- Todo el código en un solo archivo (`Program.cs`)
- Sin separación de responsabilidades
- Código duplicado en múltiples lugares
- Sin capas de abstracción

### 🔴 Seguridad
- Cadenas de conexión hardcodeadas y expuestas
- Sin autenticación ni autorización
- Exposición de información sensible del sistema
- Swagger habilitado en producción
- Endpoint para eliminar todos los registros sin protección

### 🔴 Base de Datos
- Sin migraciones
- Sin configuración de entidades
- Sin índices
- Conexiones no liberadas correctamente
- Problema N+1 en queries

### 🔴 Performance
- Sin async/await
- Carga de todos los registros sin paginación
- Queries ineficientes
- Múltiples conexiones innecesarias

### 🔴 Validación y Manejo de Errores
- Sin validación de datos de entrada
- Sin DTOs (exposición de entidades directamente)
- Manejo de errores que expone detalles internos
- Sin validación de parámetros

## API Endpoints (Todos con malas prácticas)

### GET /api/person
- Retorna TODOS los registros sin paginación
- Expone información sensible

### GET /api/person/{id}
- Sin validación de parámetros
- Retorna null en lugar de 404

### POST /api/person
- Sin validación de modelo
- Retorna entidad completa con datos sensibles

### PUT /api/person/{id}
- No verifica si el registro existe
- Asignación directa sin validación

### DELETE /api/person/{id}
- Delete físico sin validaciones
- No verifica existencia

### GET /api/person/search/{name}
- Query extremadamente ineficiente
- Trae todos los registros para filtrar en memoria

### GET /api/person/debug/database
- Expone información sensible del sistema
- Sin autorización

### DELETE /api/person/deleteall
- Operación peligrosa sin protección
- Elimina todos los registros

## Cómo ejecutar (Para fines educativos)

```bash
# Restaurar paquetes
dotnet restore

# Ejecutar la aplicación
dotnet run
```

## Lo que NO debes hacer en un proyecto real

1. **Nunca** pongas cadenas de conexión en código
2. **Nunca** expongas Swagger en producción
3. **Nunca** retornes errores con stack traces
4. **Nunca** hagas queries sin paginación
5. **Nunca** crees endpoints sin autorización
6. **Nunca** expongas entidades directamente
7. **Nunca** hagas operaciones sin validación
8. **Nunca** uses delete físico sin confirmación
9. **Nunca** olvides el manejo de errores
10. **Nunca** mezcles todas las responsabilidades

## Mejores Prácticas Recomendadas

- Usar Clean Architecture
- Implementar Repository Pattern
- Usar DTOs para transferencia de datos
- Implementar validaciones robustas
- Usar async/await
- Implementar paginación
- Usar Entity Framework correctamente
- Implementar logging apropiado
- Usar autenticación y autorización
- Implementar manejo de errores global