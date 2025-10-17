-- MALA PRÁCTICA: Script SQL con malas prácticas deliberadas
-- No usar en producción

-- MALA PRÁCTICA: Sin verificar si la base existe
CREATE DATABASE BadDatabase;
USE BadDatabase;

-- MALA PRÁCTICA: Tabla sin índices, sin restricciones apropiadas
CREATE TABLE Persons (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(MAX), -- MALA PRÁCTICA: Sin restricción de longitud
    Email nvarchar(MAX), -- MALA PRÁCTICA: Sin validación de formato
    Age int, -- MALA PRÁCTICA: Sin restricciones de rango
    Phone nvarchar(MAX), -- MALA PRÁCTICA: Sin formato específico
    Address nvarchar(MAX), -- MALA PRÁCTICA: Sin normalización
    CreatedDate datetime,
    IsActive bit
);

-- MALA PRÁCTICA: Insertar datos sin validación
INSERT INTO Persons (Name, Email, Age, Phone, Address, CreatedDate, IsActive) VALUES
('Juan Pérez', 'juan@email.com', 25, '123456789', 'Calle Falsa 123', GETDATE(), 1),
('María González', 'maria@email.com', 30, '987654321', 'Avenida Siempre Viva 456', GETDATE(), 1),
('Pedro López', 'pedro@email.com', 35, '555555555', 'Plaza Central 789', GETDATE(), 1),
('Ana Martínez', 'ana@email.com', 28, '111111111', 'Calle Real 321', GETDATE(), 1),
('Carlos Rodríguez', 'carlos@email.com', 42, '222222222', 'Avenida Principal 654', GETDATE(), 1);

-- MALA PRÁCTICA: Usuario con permisos excesivos
CREATE LOGIN baduser WITH PASSWORD = '123456';
CREATE USER baduser FOR LOGIN baduser;
ALTER ROLE db_owner ADD MEMBER baduser;

-- MALA PRÁCTICA: Stored procedure sin validaciones
CREATE PROCEDURE sp_DeleteAllPersons
AS
BEGIN
    DELETE FROM Persons;
END;

-- MALA PRÁCTICA: Función que expone información sensible
CREATE FUNCTION fn_GetConnectionInfo()
RETURNS nvarchar(MAX)
AS
BEGIN
    RETURN 'Server: ' + @@SERVERNAME + ', Database: ' + DB_NAME() + ', User: ' + USER_NAME();
END;