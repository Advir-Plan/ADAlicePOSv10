-- Script SQL para criar a tabela TDU_ALICE
-- Esta tabela guarda as configurações do terminal de pagamento Alice

-- Verificar se a tabela já existe e apagá-la (opcional, comentar se não quiser apagar)
-- IF OBJECT_ID('TDU_ALICE', 'U') IS NOT NULL
--     DROP TABLE TDU_ALICE;
-- GO

-- Criar a tabela TDU_ALICE
CREATE TABLE TDU_ALICE (
    CDU_id INT IDENTITY(1,1) PRIMARY KEY,
    CDU_BASE_URL NVARCHAR(255) NOT NULL DEFAULT 'https://192.168.1.83:8081/api',
    CDU_USER NVARCHAR(100) NOT NULL DEFAULT '8957_Admin',
    CDU_PASSWORD NVARCHAR(100) NOT NULL DEFAULT '3603ee',
    CDU_POLLING_INTERNAL_MS INT NOT NULL DEFAULT 500,
    CDU_MAX_POLLING_TIME_MS INT NOT NULL DEFAULT 300000
);
GO

-- Inserir valores padrão (opcional)
INSERT INTO TDU_ALICE (
    CDU_BASE_URL,
    CDU_USER,
    CDU_PASSWORD,
    CDU_POLLING_INTERNAL_MS,
    CDU_MAX_POLLING_TIME_MS
) VALUES (
    'https://192.168.1.83:8081/api',
    '8957_Admin',
    '3603ee',
    500,
    300000
);
GO

-- Verificar os dados inseridos
SELECT
    CDU_id,
    CDU_BASE_URL,
    CDU_USER,
    CDU_PASSWORD,
    CDU_POLLING_INTERNAL_MS,
    CDU_MAX_POLLING_TIME_MS
FROM TDU_ALICE;
GO
