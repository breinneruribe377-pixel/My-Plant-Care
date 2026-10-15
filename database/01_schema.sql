/* =====================================================================
   MyPlantCare - Script DDL (SQL Server 2019+ / Express)
   Crea la base de datos db_usuario y las tablas: usuario, planta, cuidado.
   Es idempotente: se puede ejecutar varias veces sin romper nada.
   ===================================================================== */

IF DB_ID(N'db_usuario') IS NULL
    CREATE DATABASE db_usuario;
GO

USE db_usuario;
GO

/* ---------------------------------------------------------------------
   usuario: dueño de las plantas (autenticación con JWT)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.usuario', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.usuario (
        id              INT IDENTITY(1,1) PRIMARY KEY,
        nombre          NVARCHAR(100) NOT NULL,
        email           NVARCHAR(150) NOT NULL,
        password_hash   NVARCHAR(255) NOT NULL,
        fecha_registro  DATETIME      NOT NULL CONSTRAINT DF_usuario_fecha_registro DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_usuario_email UNIQUE (email)
    );
END
GO

/* ---------------------------------------------------------------------
   planta (tabla maestra): pertenece a un usuario
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.planta', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.planta (
        id              INT IDENTITY(1,1) PRIMARY KEY,
        usuario_id      INT           NOT NULL,
        nombre          NVARCHAR(100) NOT NULL,
        especie         NVARCHAR(100) NULL,
        ubicacion       NVARCHAR(100) NULL,
        foto            NVARCHAR(255) NULL,
        fecha_creacion  DATETIME      NOT NULL CONSTRAINT DF_planta_fecha_creacion DEFAULT GETUTCDATE(),
        CONSTRAINT FK_planta_usuario FOREIGN KEY (usuario_id)
            REFERENCES dbo.usuario(id) ON DELETE CASCADE
    );

    CREATE INDEX IX_planta_usuario_id ON dbo.planta(usuario_id);
END
GO

/* ---------------------------------------------------------------------
   cuidado: tareas periódicas de una planta (riego, poda, etc.)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.cuidado', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.cuidado (
        id                    INT IDENTITY(1,1) PRIMARY KEY,
        planta_id             INT           NOT NULL,
        tipo                  NVARCHAR(30)  NOT NULL,
        frecuencia_dias       INT           NOT NULL,
        ultima_vez_realizado  DATETIME      NULL,
        proxima_fecha         DATETIME      NOT NULL,
        CONSTRAINT FK_cuidado_planta FOREIGN KEY (planta_id)
            REFERENCES dbo.planta(id) ON DELETE CASCADE,
        CONSTRAINT CK_cuidado_tipo CHECK (tipo IN ('riego','fertilizacion','poda','rotacion','trasplante','revision_plagas')),
        CONSTRAINT CK_cuidado_frecuencia CHECK (frecuencia_dias > 0)
    );

    CREATE INDEX IX_cuidado_planta_id     ON dbo.cuidado(planta_id);
    CREATE INDEX IX_cuidado_proxima_fecha ON dbo.cuidado(proxima_fecha);
END
GO
