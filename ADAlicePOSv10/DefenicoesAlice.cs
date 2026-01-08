using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ADAlicePOSv10.Licensing;

namespace ADAlicePOSv10
{
    public partial class DefenicoesAlice : Form
    {
        private ErpBS100.ErpBS _bSO;
        private StdPlatBS100.StdBSInterfPub _pSO;

        public DefenicoesAlice(ErpBS100.ErpBS bSO, StdPlatBS100.StdBSInterfPub pSO)
        {
            InitializeComponent();
            _bSO = bSO;
            _pSO = pSO;

            // Carregar dados quando o form abre
            CarregarDados();
            CarregarInfoLicenca();
        }

        /// <summary>
        /// Carrega os dados da base de dados para os campos do formulário
        /// </summary>
        private void CarregarDados()
        {
            try
            {
                var queryCarregar = "SELECT CDU_BASE_URL, CDU_USER, CDU_PASSWORD, CDU_POLLING_INTERNAL_MS, CDU_MAX_POLLING_TIME_MS FROM TDU_ALICE";
                var resultado = _bSO.Consulta(queryCarregar);

                // Verificar se existe algum registo
                if (!resultado.Vazia())
                {
                    resultado.Inicio();

                    // Preencher os campos com os valores da BD
                    txtBaseUrl.Text = resultado.Valor("CDU_BASE_URL").ToString();
                    txtUser.Text = resultado.Valor("CDU_USER").ToString();
                    txtPassword.Text = resultado.Valor("CDU_PASSWORD").ToString();
                    numPollingInterval.Value = Convert.ToDecimal(resultado.Valor("CDU_POLLING_INTERNAL_MS"));
                    numMaxPollingTime.Value = Convert.ToDecimal(resultado.Valor("CDU_MAX_POLLING_TIME_MS"));
                }
                else
                {
                    // Se não existir, preencher com valores padrão
                    txtBaseUrl.Text = "https://192.168.1.84:8081/api";
                    txtUser.Text = "8957_Admin";
                    txtPassword.Text = "3603ee";
                    numPollingInterval.Value = 500;
                    numMaxPollingTime.Value = 300000;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar definições: " + ex.Message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Se der erro, preencher com valores padrão
                txtBaseUrl.Text = "https://192.168.1.84:8081/api";
                txtUser.Text = "8957_Admin";
                txtPassword.Text = "3603ee";
                numPollingInterval.Value = 500;
                numMaxPollingTime.Value = 300000;
            }
        }

        /// <summary>
        /// Carrega informações da licença para exibir no formulário
        /// </summary>
        private void CarregarInfoLicenca()
        {
            try
            {
                // Obter data de validade da licença
                DateTime? dataValidade = Licensing.LicenseManager.GetLicenseExpirationDate();

                if (dataValidade.HasValue)
                {
                    lblLicenseExpiry.Text = $"Validade da Licença: {dataValidade.Value:dd/MM/yyyy}";

                    // Calcular dias restantes
                    TimeSpan diasRestantes = dataValidade.Value - DateTime.Now;
                    int dias = (int)Math.Ceiling(diasRestantes.TotalDays);

                    // Atualizar label de dias restantes
                    if (dias > 0)
                    {
                        lblDiasRestantes.Text = $"{dias} {(dias == 1 ? "dia restante" : "dias restantes")}";

                        // Cor baseada nos dias restantes
                        if (dias <= 7)
                        {
                            lblDiasRestantes.ForeColor = Color.Red;
                            lblLicenseExpiry.ForeColor = Color.Red;
                        }
                        else if (dias <= 30)
                        {
                            lblDiasRestantes.ForeColor = Color.Orange;
                            lblLicenseExpiry.ForeColor = Color.Orange;
                        }
                        else
                        {
                            lblDiasRestantes.ForeColor = Color.Green;
                            lblLicenseExpiry.ForeColor = Color.Green;
                        }
                    }
                    else
                    {
                        lblLicenseExpiry.Text = "Licença EXPIRADA!";
                        lblLicenseExpiry.ForeColor = Color.Red;
                        lblDiasRestantes.Text = "Licença expirada";
                        lblDiasRestantes.ForeColor = Color.Red;
                    }
                }
                else
                {
                    lblLicenseExpiry.Text = "Licença: Não encontrada";
                    lblLicenseExpiry.ForeColor = Color.Red;
                    lblDiasRestantes.Text = "Licença não encontrada";
                    lblDiasRestantes.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblLicenseExpiry.Text = "Licença: Erro ao carregar";
                lblLicenseExpiry.ForeColor = Color.Gray;
                lblDiasRestantes.Text = "Erro ao carregar";
                lblDiasRestantes.ForeColor = Color.Gray;
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar info da licença: {ex.Message}");
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Validar campos obrigatórios
                if (string.IsNullOrWhiteSpace(txtBaseUrl.Text))
                {
                    MessageBox.Show("O campo 'URL Base da API' é obrigatório.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtBaseUrl.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtUser.Text))
                {
                    MessageBox.Show("O campo 'Utilizador' é obrigatório.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUser.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("O campo 'Password' é obrigatório.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                // Obter valores dos campos
                string baseUrl = txtBaseUrl.Text.Trim();
                string user = txtUser.Text.Trim();
                string password = txtPassword.Text.Trim();
                int pollingInterval = (int)numPollingInterval.Value;
                int maxPollingTime = (int)numMaxPollingTime.Value;

                // Escapar aspas simples para prevenir SQL injection
                baseUrl = baseUrl.Replace("'", "''");
                user = user.Replace("'", "''");
                password = password.Replace("'", "''");

                // Verificar se já existe registo na tabela
                var queryVerificaExiste = "SELECT COUNT(*) as 'count' FROM TDU_ALICE";
                var resultadoCount = _bSO.Consulta(queryVerificaExiste);
                resultadoCount.Inicio();
                int count = Convert.ToInt32(resultadoCount.Valor("count"));

                if (count == 0)
                {
                    // Inserir novo registo
                    var queryINSERT = $@"
                        INSERT INTO TDU_ALICE (CDU_id, CDU_BASE_URL, CDU_USER, CDU_PASSWORD, CDU_POLLING_INTERNAL_MS, CDU_MAX_POLLING_TIME_MS)
                        VALUES (1, '{baseUrl}', '{user}', '{password}', {pollingInterval}, {maxPollingTime})";

                    _bSO.DSO.ExecuteSQL(queryINSERT);
                }
                else
                {
                    // Atualizar registo existente
                    var queryUPDATE = $@"
                        UPDATE TDU_ALICE
                        SET CDU_BASE_URL = '{baseUrl}',
                            CDU_USER = '{user}',
                            CDU_PASSWORD = '{password}',
                            CDU_POLLING_INTERNAL_MS = {pollingInterval},
                            CDU_MAX_POLLING_TIME_MS = {maxPollingTime}
                        WHERE CDU_ID = 1";

                    _bSO.DSO.ExecuteSQL(queryUPDATE);
                }

                // Mostrar mensagem de sucesso
                MessageBox.Show("Configurações guardadas com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Fechar o form
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao guardar definições: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
