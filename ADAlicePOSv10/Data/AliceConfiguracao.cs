using System;

namespace ADAlicePOSv10.Data
{
    /// <summary>
    /// Classe que representa as configurações da Alice guardadas na base de dados
    /// </summary>
    public class AliceConfiguracao
    {
        public int CDU_id { get; set; }
        public string CDU_BASE_URL { get; set; }
        public string CDU_USER { get; set; }
        public string CDU_PASSWORD { get; set; }
        public int CDU_POLLING_INTERNAL_MS { get; set; }
        public int CDU_MAX_POLLING_TIME_MS { get; set; }

        /// <summary>
        /// Construtor padrão com valores default
        /// </summary>
        public AliceConfiguracao()
        {
            CDU_BASE_URL = "https://192.168.1.83:8081/api";
            CDU_USER = "8957_Admin";
            CDU_PASSWORD = "3603ee";
            CDU_POLLING_INTERNAL_MS = 500;
            CDU_MAX_POLLING_TIME_MS = 300000;
        }
    }
}
