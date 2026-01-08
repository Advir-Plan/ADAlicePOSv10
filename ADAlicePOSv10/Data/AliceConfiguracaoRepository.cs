using System;
using System.Data;
using Primavera.Extensibility.BusinessEntities.ExtensibilityService.EventArgs;

namespace ADAlicePOSv10.Data
{
    /// <summary>
    /// Repositório para acesso às configurações da Alice na base de dados
    /// </summary>
    public class AliceConfiguracaoRepository
    {
        private readonly Primavera.Extensibility.BusinessEntities.ExtensibilityPattern.PriExtensibility extensibility;

        public AliceConfiguracaoRepository(Primavera.Extensibility.BusinessEntities.ExtensibilityPattern.PriExtensibility ext)
        {
            this.extensibility = ext;
        }

        /// <summary>
        /// Carrega a configuração da base de dados. Se não existir, retorna configuração padrão.
        /// </summary>
        public AliceConfiguracao Carregar()
        {
            try
            {
                var query = "SELECT CDU_id, CDU_BASE_URL, CDU_USER, CDU_PASSWORD, CDU_POLLING_INTERNAL_MS, CDU_MAX_POLLING_TIME_MS FROM TDU_ALICE";

                var dt = extensibility.PSO.Execute(query).ToDataTable();

                if (dt != null && dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    return new AliceConfiguracao
                    {
                        CDU_id = row["CDU_id"] != DBNull.Value ? Convert.ToInt32(row["CDU_id"]) : 0,
                        CDU_BASE_URL = row["CDU_BASE_URL"]?.ToString() ?? "https://192.168.1.83:8081/api",
                        CDU_USER = row["CDU_USER"]?.ToString() ?? "8957_Admin",
                        CDU_PASSWORD = row["CDU_PASSWORD"]?.ToString() ?? "3603ee",
                        CDU_POLLING_INTERNAL_MS = row["CDU_POLLING_INTERNAL_MS"] != DBNull.Value ? Convert.ToInt32(row["CDU_POLLING_INTERNAL_MS"]) : 500,
                        CDU_MAX_POLLING_TIME_MS = row["CDU_MAX_POLLING_TIME_MS"] != DBNull.Value ? Convert.ToInt32(row["CDU_MAX_POLLING_TIME_MS"]) : 300000
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar configuração: {ex.Message}");
            }

            // Retorna configuração padrão se não encontrar na BD
            return new AliceConfiguracao();
        }

        /// <summary>
        /// Guarda ou atualiza a configuração na base de dados
        /// </summary>
        public bool Guardar(AliceConfiguracao config)
        {
            try
            {
                // Verificar se já existe um registo
                var existeQuery = "SELECT COUNT(*) as Total FROM TDU_ALICE";
                var dtExiste = extensibility.PSO.Execute(existeQuery).ToDataTable();

                bool existeRegisto = false;
                if (dtExiste != null && dtExiste.Rows.Count > 0)
                {
                    existeRegisto = Convert.ToInt32(dtExiste.Rows[0]["Total"]) > 0;
                }

                string query;
                if (existeRegisto)
                {
                    // Atualizar registo existente
                    query = $@"UPDATE TDU_ALICE SET
                        CDU_BASE_URL = '{EscapeSql(config.CDU_BASE_URL)}',
                        CDU_USER = '{EscapeSql(config.CDU_USER)}',
                        CDU_PASSWORD = '{EscapeSql(config.CDU_PASSWORD)}',
                        CDU_POLLING_INTERNAL_MS = {config.CDU_POLLING_INTERNAL_MS},
                        CDU_MAX_POLLING_TIME_MS = {config.CDU_MAX_POLLING_TIME_MS}";
                }
                else
                {
                    // Inserir novo registo
                    query = $@"INSERT INTO TDU_ALICE
                        (CDU_BASE_URL, CDU_USER, CDU_PASSWORD, CDU_POLLING_INTERNAL_MS, CDU_MAX_POLLING_TIME_MS)
                        VALUES (
                            '{EscapeSql(config.CDU_BASE_URL)}',
                            '{EscapeSql(config.CDU_USER)}',
                            '{EscapeSql(config.CDU_PASSWORD)}',
                            {config.CDU_POLLING_INTERNAL_MS},
                            {config.CDU_MAX_POLLING_TIME_MS}
                        )";
                }

                extensibility.PSO.Execute(query);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao guardar configuração: {ex.Message}");
                throw new Exception($"Erro ao guardar configurações: {ex.Message}");
            }
        }

        /// <summary>
        /// Escapa aspas simples para prevenir SQL injection
        /// </summary>
        private string EscapeSql(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("'", "''");
        }
    }
}
