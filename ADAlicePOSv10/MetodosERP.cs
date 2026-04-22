using Primavera.Extensibility.CustomCode;
using Primavera.Extensibility.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADAlicePOSv10
{
    public class MetodosERP : CustomCode
    {
        /// <summary>
        /// Abre o editor de definições da Alice
        /// Verifica e cria a tabela TDU_ALICE se não existir
        /// </summary>
        public void AbreEditorDefenicoes()
        {
            try
            {
                // Verificar e criar tabela se necessário
                VerificarECriarTabelaAlice();

                // Abrir o editor


                DefenicoesAlice defenicoesAlice = new DefenicoesAlice(BSO, PSO);
                defenicoesAlice.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao abrir configurações:\n{ex.Message}",
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Verifica se a tabela TDU_ALICE existe e cria se necessário
        /// </summary>
        private void VerificarECriarTabelaAlice()
        {
            try
            {
                // Verificar se a tabela existe
                if (!TabelaExiste("TDU_ALICE"))
                {
                    CriarTabelaAlice();
                    MessageBox.Show(
                        "Tabela de configurações da Alice criada com sucesso!",
                        "Configuração Inicial",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    // Verificar se todos os campos existem
                    VerificarECriarCampos();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Alice] Erro ao verificar/criar tabela: {ex.Message}");
                throw new Exception($"Erro ao preparar configurações: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se uma tabela existe na base de dados
        /// </summary>
        private bool TabelaExiste(string nomeTabela)
        {
            try
            {
                var query = $"SELECT COUNT(*) as Total FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{nomeTabela}'";
                var resultado = BSO.Consulta(query);

                if (!resultado.Vazia())
                {
                    resultado.Inicio();
                    int count = Convert.ToInt32(resultado.Valor("Total"));
                    return count > 0;
                }

                return false;
            }
            catch
            {
                // Se der erro, assume que não existe
                return false;
            }
        }

        /// <summary>
        /// Cria a tabela TDU_ALICE com todos os campos necessários
        /// </summary>
        private void CriarTabelaAlice()
        {
            try
            {
                var sqlCreate = @"
                    CREATE TABLE TDU_ALICE (
                        CDU_id INT PRIMARY KEY IDENTITY(1,1),
                        CDU_BASE_URL NVARCHAR(255) NULL,
                        CDU_USER NVARCHAR(255) NULL,
                        CDU_PASSWORD NVARCHAR(255) NULL,
                        CDU_POLLING_INTERNAL_MS INT NULL DEFAULT 500,
                        CDU_MAX_POLLING_TIME_MS INT NULL DEFAULT 300000,
                        CDU_LICENSE_EXPIRY DATETIME NULL,
                        CDU_DATA_CRIACAO DATETIME DEFAULT GETDATE(),
                        CDU_DATA_ATUALIZACAO DATETIME DEFAULT GETDATE()
                    )";

                BSO.DSO.ExecuteSQL(sqlCreate);

                // Inserir configurações padrão
                InserirConfiguracoesDefault();

                System.Diagnostics.Debug.WriteLine("[Alice] Tabela TDU_ALICE criada com sucesso");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Alice] Erro ao criar tabela: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Verifica e cria campos que possam estar em falta
        /// </summary>
        private void VerificarECriarCampos()
        {
            try
            {
                var camposNecessarios = new Dictionary<string, string>
                {
                    { "CDU_BASE_URL", "NVARCHAR(255) NULL" },
                    { "CDU_USER", "NVARCHAR(255) NULL" },
                    { "CDU_PASSWORD", "NVARCHAR(255) NULL" },
                    { "CDU_POLLING_INTERNAL_MS", "INT NULL" },
                    { "CDU_MAX_POLLING_TIME_MS", "INT NULL" },
                    { "CDU_LICENSE_EXPIRY", "DATETIME NULL" },
                    { "CDU_DATA_CRIACAO", "DATETIME NULL" },
                    { "CDU_DATA_ATUALIZACAO", "DATETIME NULL" }
                };

                foreach (var campo in camposNecessarios)
                {
                    if (!CampoExiste("TDU_ALICE", campo.Key))
                    {
                        AdicionarCampo("TDU_ALICE", campo.Key, campo.Value);
                        System.Diagnostics.Debug.WriteLine($"[Alice] Campo {campo.Key} adicionado");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Alice] Erro ao verificar campos: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se um campo existe numa tabela
        /// </summary>
        private bool CampoExiste(string nomeTabela, string nomeCampo)
        {
            try
            {
                var query = $@"
                    SELECT COUNT(*) as Total
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = '{nomeTabela}'
                    AND COLUMN_NAME = '{nomeCampo}'";

                var resultado = BSO.Consulta(query);

                if (!resultado.Vazia())
                {
                    resultado.Inicio();
                    int count = Convert.ToInt32(resultado.Valor("Total"));
                    return count > 0;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Adiciona um campo a uma tabela existente
        /// </summary>
        private void AdicionarCampo(string nomeTabela, string nomeCampo, string tipoCampo)
        {
            try
            {
                var sqlAlter = $"ALTER TABLE {nomeTabela} ADD {nomeCampo} {tipoCampo}";
                BSO.DSO.ExecuteSQL(sqlAlter);

                System.Diagnostics.Debug.WriteLine($"[Alice] Campo {nomeCampo} adicionado à tabela {nomeTabela}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Alice] Erro ao adicionar campo {nomeCampo}: {ex.Message}");
            }
        }

        /// <summary>
        /// Insere configurações padrão na tabela
        /// </summary>
        private void InserirConfiguracoesDefault()
        {
            try
            {
                var sqlInsert = @"
                    INSERT INTO TDU_ALICE (
                        CDU_BASE_URL,
                        CDU_USER,
                        CDU_PASSWORD,
                        CDU_POLLING_INTERNAL_MS,
                        CDU_MAX_POLLING_TIME_MS
                    )
                    VALUES (
                        'https://192.168.1.84:8081/api',
                        '8957_Admin',
                        '3603ee',
                        500,
                        300000
                    )";

                BSO.DSO.ExecuteSQL(sqlInsert);

                System.Diagnostics.Debug.WriteLine("[Alice] Configurações padrão inseridas");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Alice] Erro ao inserir configurações padrão: {ex.Message}");
            }
        }
    }
}
