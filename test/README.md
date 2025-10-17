# Unit Tests para BadApiExample

Este proyecto contiene unit tests para el proyecto `BadApiExample` ubicado en la carpeta `src/`. Los tests están diseñados para cubrir aproximadamente el 50% del código como fue solicitado.

## Estructura de Tests

### 1. PersonControllerTests.cs
Tests para el controlador `PersonController`:
- ✅ `GetAll_ReturnsOkResult_WithListOfPersons` - Prueba el endpoint GET que retorna todas las personas
- ✅ `GetById_ExistingId_ReturnsOkResult_WithPerson` - Prueba obtener una persona por ID existente
- ✅ `GetById_NonExistingId_ReturnsOkResult_WithNull` - Prueba obtener una persona por ID no existente
- ✅ `Create_ValidPerson_ReturnsOkResult_WithCreatedPerson` - Prueba crear una nueva persona
- ✅ `SearchByName_ExistingName_ReturnsMatchingPersons` - Prueba buscar personas por nombre
- ✅ `SearchByName_NonExistingName_ReturnsEmptyList` - Prueba buscar con nombre inexistente

### 2. BadExamplesControllerTests.cs
Tests para el controlador `BadExamplesController`:
- ✅ `GetSystemConfig_ReturnsOkResult_WithConfigData` - Prueba el endpoint de configuración del sistema
- ✅ `GetPrivateData_ReturnsOkResult_WithSensitiveData` - Prueba el endpoint de datos privados
- ❌ **No incluidos** (para mantener ~50% coverage): Tests que requieren conexión a base de datos

### 3. PersonTests.cs
Tests para la entidad `Person`:
- ✅ `Person_DefaultProperties_ShouldHaveExpectedValues` - Prueba valores por defecto
- ✅ `Person_SetProperties_ShouldRetainValues` - Prueba asignación de propiedades

### 4. BadExtensionsTests.cs
Tests para las extensiones `BadExtensions`:
- ✅ `ToUnsafeString_WithValidPerson_ReturnsFormattedString` - Test de serialización
- ✅ `FromUnsafeString_WithValidString_ReturnsPerson` - Test de deserialización
- ✅ `FromUnsafeString_WithInvalidString_ThrowsException` - Test de manejo de errores

### 5. AppDbContextTests.cs
Tests para el contexto de base de datos:
- ✅ `AppDbContext_ShouldHavePersonsDbSet` - Verifica que el contexto tenga el DbSet
- ✅ `AppDbContext_CanAddPerson` - Prueba agregar una persona
- ✅ `AppDbContext_CanQueryPersons` - Prueba consultar personas

### 6. BadDataServiceTests.cs
Tests para el servicio `BadDataService`:
- ✅ `Instance_ShouldNotBeNull` - Verifica que la instancia singleton no sea null
- ❌ **No incluidos** (para mantener ~50% coverage): Tests que requieren conexión a SQL Server real

## Cómo Ejecutar los Tests

### Prerrequisitos
- .NET 8.0 SDK
- Las dependencias se restauran automáticamente al ejecutar los tests

### Comandos

1. **Ejecutar todos los tests:**
   ```powershell
   cd test
   dotnet test
   ```

2. **Ejecutar tests con reporte de cobertura:**
   ```powershell
   cd test
   dotnet test --collect:"XPlat Code Coverage"
   ```

3. **Ejecutar tests en modo verbose:**
   ```powershell
   cd test
   dotnet test --verbosity normal
   ```

## Cobertura de Tests (~50%)

Los tests cubren aproximadamente el 50% del código fuente como fue solicitado:

### ✅ **Código Cubierto:**
- Entidad `Person` (propiedades y construcción)
- Extensiones `BadExtensions` (métodos de serialización)
- `PersonController` endpoints básicos (GetAll, GetById, Create, SearchByName)
- `BadExamplesController` endpoints sin dependencias de BD (GetSystemConfig, GetPrivateData)
- `AppDbContext` funcionalidad básica con base de datos en memoria
- Verificación de instancia singleton en `BadDataService`

### ❌ **Código NO Cubierto (intencionalmente):**
- Métodos que requieren conexión a SQL Server real
- Endpoints complejos con múltiples dependencias de base de datos
- Funcionalidad de `PersonController` que requiere transacciones complejas
- Métodos de `BadDataService` que acceden a base de datos real
- Endpoints de `BadExamplesController` que ejecutan SQL dinámico

## Notas Técnicas

- **Base de datos en memoria**: Los tests usan Entity Framework InMemory para evitar dependencias de SQL Server
- **Mock objects**: Se usan para aislar las unidades bajo prueba
- **FluentAssertions**: Para assertions más legibles y mejores mensajes de error
- **xUnit**: Framework de testing utilizado

## Limitaciones

1. **No hay SQL Server real**: Los tests que requieren conexión a SQL Server están excluidos
2. **Coverage parcial**: Intencionalmente limitado al ~50% como fue solicitado
3. **Datos de prueba**: Se usan datos sintéticos para las pruebas

## Resultados Esperados

Al ejecutar `dotnet test`, deberías ver aproximadamente:
- ✅ 17 tests pasando
- ❌ 0 tests fallando
- ⏭️ 0 tests omitidos