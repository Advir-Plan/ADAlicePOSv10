using Primavera.Extensibility.POS.Editors;
using Primavera.Extensibility.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Primavera.Extensibility.BusinessEntities.ExtensibilityService.EventArgs;
using ADAlicePOSv10.Licensing;

namespace ADAlicePOSv10.POS
{
    public class UiEditorVendas : EditorVendas
    {
        // Variáveis para guardar as configurações da Alice carregadas da BD
        private string ALICE_BASE_URL = "https://192.168.1.84:8081/api";
        private string ALICE_USER = "8957_Admin";
        private string ALICE_PASSWORD = "3603ee";
        private int POLLING_INTERVAL_MS = 500;
        private int MAX_POLLING_TIME_MS = 300000;

        private FormPagamentoAlice formPagamento;

        /// <summary>
        /// Construtor - Valida a licença apenas UMA vez quando o módulo é carregado
        /// </summary>
        public UiEditorVendas()
        {
            try
            {
                LicenseManager.ValidateLicense();
            }
            catch (LicenseException ex)
            {
                // Criar mensagem de erro formatada e simplificada
                string mensagemErro = $"LICENÇA INVÁLIDA OU EXPIRADA\n\n" +
                                     $"{ex.Message}\n\n" +
                                     $"═══════════════════════════════════════════════\n\n" +
                                     $"Para obter a licença, contacte a equipa Advir:\n\n" +
                                     $"📧 Email: support@advir.pt\n" +
                                     $"📞 Telefone: 253 038 244\n" +
                                     $"🌐 Web: https://advir.pt/";

                MessageBox.Show(
                    mensagemErro,
                    "ADAlice POS - Licença Inválida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                // Lança exceção para impedir o carregamento do módulo
                throw;
            }
        }

        /// <summary>
        /// Carrega as configurações da Alice da base de dados
        /// </summary>
        private void CarregarConfiguracoes()
        {
            try
            {
                var queryDados = "SELECT CDU_BASE_URL, CDU_USER, CDU_PASSWORD, CDU_POLLING_INTERNAL_MS, CDU_MAX_POLLING_TIME_MS FROM TDU_ALICE";
                var regDados = this.BSO.Consulta(queryDados);

                // Verificar se existem dados
                if (!regDados.Vazia())
                {
                    regDados.Inicio();


                    // Carregar os valores da BD para as variáveis
                    ALICE_BASE_URL = regDados.Valor("CDU_BASE_URL").ToString();
                    ALICE_USER = regDados.Valor("CDU_USER").ToString();
                    ALICE_PASSWORD = regDados.Valor("CDU_PASSWORD").ToString();
                    POLLING_INTERVAL_MS = Convert.ToInt32(regDados.Valor("CDU_POLLING_INTERNAL_MS"));
                    MAX_POLLING_TIME_MS = Convert.ToInt32(regDados.Valor("CDU_MAX_POLLING_TIME_MS"));

                    System.Diagnostics.Debug.WriteLine($"[Alice] Configurações carregadas: URL={ALICE_BASE_URL}, User={ALICE_USER}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[Alice] Nenhuma configuração encontrada na BD. A usar valores padrão.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Alice] Erro ao carregar configurações: {ex.Message}. A usar valores padrão.");
            }
        }

        public override void DepoisDeConfirmar(ref bool Cancel, ExtensibilityEventArgs e)
        {
            try
            {
                // Carregar configurações da BD antes de processar
                CarregarConfiguracoes();

                if (this.DocumentoVenda.ModoPag == "NUM")
                {
                    double valorTotal = this.DocumentoVenda.TotalMerc;

                    if (valorTotal <= 0)
                    {
                        this.PSO.MensagensDialogos.MostraAviso(
                            $"Erro: Valor inválido ({valorTotal:C2}).\n\nPor favor, verifique o total da venda.",
                            StdBE100.StdBETipos.IconId.PRI_Critico
                        );
                        Cancel = true;
                        return;
                    }

                    // Criar form
                    formPagamento = new FormPagamentoAlice(valorTotal);

                    // Assinar evento de carregamento
                    formPagamento.Shown += async (s, ev) =>
                    {
                        await ProcessarPagamentoAlice(valorTotal);
                    };

                    // Mostrar modal (agora vai processar enquanto mostra)
                    var resultado = formPagamento.ShowDialog();

                    if (formPagamento.Cancelado || resultado != DialogResult.OK)
                    {
                        Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                this.PSO.MensagensDialogos.MostraAviso(
                    $"Erro ao processar pagamento:\n\n{ex.Message}",
                    StdBE100.StdBETipos.IconId.PRI_Critico
                );
                Cancel = true;
            }
        }

        private async Task ProcessarPagamentoAlice(double valor)
        {
            try
            {
                var resultado = await IniciarOperacaoSale(valor);

                if (!resultado.Sucesso)
                {
                    formPagamento?.MostrarErro($"Erro ao iniciar operação:\n{resultado.Mensagem}");
                    return;
                }

                await MonitorarOperacaoComProgresso(resultado.OperationId, valor);
            }
            catch (Exception ex)
            {
                formPagamento?.MostrarErro($"Erro no processamento:\n{ex.Message}");
            }
        }

        private async Task<ResultadoAlice> IniciarOperacaoSale(double valor)
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback =
                        (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(handler))
                    {
                        var authToken = Convert.ToBase64String(
                            Encoding.ASCII.GetBytes($"{ALICE_USER}:{ALICE_PASSWORD}")
                        );
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Basic", authToken);

                        client.Timeout = TimeSpan.FromSeconds(30);

                        int valorCentavos = (int)Math.Round(valor * 100);

                        var payload = new
                        {
                            operation = new
                            {
                                amount = valorCentavos,
                                operator_id = Environment.UserName,
                                terminal_id = Environment.MachineName,
                                observations = $"Venda POS - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                                reference = this.DocumentoVenda?.NumDoc.ToString() ?? string.Empty
                            }
                        };

                        var json = JsonConvert.SerializeObject(payload);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync($"{ALICE_BASE_URL}/operation/sale", content);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            var resultadoResp = JsonConvert.DeserializeObject<AliceResponse>(responseContent);
                            return new ResultadoAlice
                            {
                                Sucesso = true,
                                OperationId = resultadoResp?.data?.operation?.operation_id ?? 0,
                                Mensagem = "Operação iniciada com sucesso"
                            };
                        }
                        else
                        {
                            return new ResultadoAlice
                            {
                                Sucesso = false,
                                Mensagem = $"Erro HTTP {response.StatusCode}: {responseContent}"
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResultadoAlice { Sucesso = false, Mensagem = ex.Message };
            }
        }

        private async Task MonitorarOperacaoComProgresso(int operationId, double valorEsperado)
        {
            var startTime = DateTime.Now;
            decimal ultimoValorRecebido = 0;

            System.Diagnostics.Debug.WriteLine($">>> Iniciando monitoramento da operação {operationId}");

            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback =
                        (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(handler))
                    {
                        var authToken = Convert.ToBase64String(
                            Encoding.ASCII.GetBytes($"{ALICE_USER}:{ALICE_PASSWORD}")
                        );
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Basic", authToken);

                        client.Timeout = TimeSpan.FromSeconds(10);

                        int tentativas = 0;

                        while ((DateTime.Now - startTime).TotalMilliseconds < MAX_POLLING_TIME_MS)
                        {
                            tentativas++;

                            // Verificar cancelamento
                            if (formPagamento?.Cancelado == true)
                            {
                                System.Diagnostics.Debug.WriteLine(">>> Operação cancelada pelo usuário");
                                await CancelarOperacaoAlice(operationId);
                                formPagamento?.MostrarErro("Operação cancelada.\nDinheiro devolvido.");
                                break;
                            }

                            try
                            {
                                var response = await client.GetAsync(
                                    $"{ALICE_BASE_URL}/operation?operation_id={operationId}"
                                );

                                if (response.IsSuccessStatusCode)
                                {
                                    var content = await response.Content.ReadAsStringAsync();

                                    // LOG: Ver resposta
                                    if (tentativas % 10 == 0) // Log a cada 10 tentativas (5 segundos)
                                    {
                                        System.Diagnostics.Debug.WriteLine($">>> Polling #{tentativas}: {content}");
                                    }

                                    var pollingResponse = JsonConvert.DeserializeObject<AlicePollingResponse>(content);

                                    if (pollingResponse?.data?.operation != null)
                                    {
                                        var operation = pollingResponse.data.operation;
                                        var state = operation.state;
                                        var valorRecebido = operation.total_in ?? 0;

                                        System.Diagnostics.Debug.WriteLine($">>> Estado: {state}, Valor recebido: {valorRecebido}, Último: {ultimoValorRecebido}");

                                        // DEBUG: Verificar se há mensagens
                                        System.Diagnostics.Debug.WriteLine($">>> pollingResponse.messages != null? {pollingResponse.messages != null}");
                                        if (pollingResponse.messages != null)
                                        {
                                            System.Diagnostics.Debug.WriteLine($">>> pollingResponse.messages.Count = {pollingResponse.messages.Count}");
                                        }

                                        // Verificar se há mensagens de aviso (WARN)
                                        if (pollingResponse.messages != null && pollingResponse.messages.Count > 0)
                                        {
                                            System.Diagnostics.Debug.WriteLine($">>> ENTROU NO BLOCO DE MENSAGENS! Total: {pollingResponse.messages.Count}");

                                            foreach (var msg in pollingResponse.messages)
                                            {
                                                System.Diagnostics.Debug.WriteLine($">>> MENSAGEM RAW: [{msg ?? "NULL"}]");

                                                if (!string.IsNullOrEmpty(msg))
                                                {
                                                    // Filtrar mensagens técnicas de status (não são avisos reais)
                                                    if (msg.Contains("Status for operation") ||
                                                        msg.Contains("operation id") && msg.Contains("received"))
                                                    {
                                                        System.Diagnostics.Debug.WriteLine($">>> Status técnico (ignorado): {msg}");
                                                        continue; // Ignora mensagens de status técnico
                                                    }

                                                    System.Diagnostics.Debug.WriteLine($">>> AVISO ALICE (vai traduzir): {msg}");

                                                    // Traduzir código de aviso para mensagem amigável
                                                    string avisoTraduzido = TraduzirAvisoAlice(msg);

                                                    System.Diagnostics.Debug.WriteLine($">>> AVISO TRADUZIDO: {avisoTraduzido}");

                                                    // Mostrar aviso no form (sem bloquear a operação)
                                                    formPagamento?.MostrarAviso(avisoTraduzido);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine($">>> SEM MENSAGENS neste polling");
                                        }

                                        if (valorRecebido != ultimoValorRecebido || state != 1)
                                        {
                                            ultimoValorRecebido = valorRecebido;
                                            System.Diagnostics.Debug.WriteLine($">>> Atualizando progresso: {valorRecebido}");
                                            formPagamento?.AtualizarProgresso(valorRecebido, valorEsperado, state);
                                        }

                                        if (state == 4)
                                        {
                                            System.Diagnostics.Debug.WriteLine(">>> SUCESSO!");
                                            formPagamento?.MostrarSucesso(valorRecebido, operation.total_out ?? 0);
                                            return;
                                        }
                                        else if (state == 5)
                                        {
                                            System.Diagnostics.Debug.WriteLine(">>> CANCELADO!");
                                            formPagamento?.MostrarErro("Operação cancelada.\nDinheiro devolvido.");
                                            return;
                                        }
                                        else if (state == 6)
                                        {
                                            System.Diagnostics.Debug.WriteLine($">>> ERRO: fail_reason={operation.fail_reason}, fail_error={operation.fail_error}");

                                            // Traduzir fail_reason para mensagens amigáveis
                                            string mensagemErro = TraduzirErroAlice(operation.fail_reason, operation.fail_error);

                                            formPagamento?.MostrarErro(mensagemErro);
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($">>> Erro HTTP: {response.StatusCode}");
                                }

                                await Task.Delay(POLLING_INTERVAL_MS);
                            }
                            catch (Exception pollingEx)
                            {
                                System.Diagnostics.Debug.WriteLine($">>> Erro no polling: {pollingEx.Message}");
                                await Task.Delay(POLLING_INTERVAL_MS);
                            }
                        }

                        System.Diagnostics.Debug.WriteLine(">>> TIMEOUT!");

                        // Cancelar a operação e devolver o dinheiro
                        await CancelarOperacaoAlice(operationId);

                        formPagamento?.MostrarErro("Tempo limite excedido.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($">>> EXCEÇÃO: {ex.Message}");
                formPagamento?.MostrarErro($"Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Traduz os códigos de erro da Alice para mensagens amigáveis
        /// </summary>
        private string TraduzirErroAlice(int? failReason, string failError)
        {
            // Baseado na documentação da API Alice
            // fail_reason valores:
            // 0 = No change (Sem troco disponível)
            // 1 = Reject (Rejeição de moeda/nota)
            // 2 = Operation failure (Falha na operação)
            // 3 = Validate failure (Falha na validação)
            // 4 = Operation timeout (Timeout da operação)
            // 5 = User canceled (Cancelado pelo utilizador)
            // 6 = Error (Erro de hardware)

            switch (failReason)
            {
                case 0:
                    return "Troco insuficiente.\n\nO terminal não tem troco disponível.\nDinheiro devolvido.";

                case 1:
                    // Se fail_error = 0, geralmente é falta de troco
                    if (string.IsNullOrEmpty(failError) || failError == "0")
                    {
                        return "Troco insuficiente.\n\nO terminal não tem troco disponível para este valor.\nDinheiro devolvido.";
                    }
                    else
                    {
                        return "Moeda ou nota rejeitada.\n\nPor favor, tente com outra nota.\nDinheiro devolvido.";
                    }

                case 2:
                    return "Falha na operação.\n\nNão foi possível completar o pagamento.\nDinheiro devolvido.";

                case 3:
                    return "Operação não autorizada.\n\nO terminal não pode processar este pagamento.\nDinheiro devolvido.";

                case 4:
                    return "Tempo limite excedido.\n\nOperação cancelada.\nDinheiro devolvido.";

                case 5:
                    return "Operação cancelada.\n\nDinheiro devolvido.";

                case 6:
                    string codigoErro = !string.IsNullOrEmpty(failError) ? failError : "Desconhecido";
                    string descricaoErro = ObterDescricaoErroHardware(codigoErro);
                    return $"Erro no terminal de pagamento.\n\n{descricaoErro}\n\nCódigo: {codigoErro}\n\nContacte o suporte técnico.";

                default:
                    return $"Erro no pagamento.\n\nCódigo: {failReason ?? -1}\nDinheiro devolvido.";
            }
        }

        /// <summary>
        /// Traduz avisos (WARN) da Alice para mensagens amigáveis
        /// </summary>
        private string TraduzirAvisoAlice(string mensagem)
        {
            // Extrair código do aviso (formato: "2203 - Warn Bill Pulled Backwards")
            if (mensagem.Contains("2203") || mensagem.Contains("Pulled Backwards"))
                return "Nota puxada para trás.";

            if (mensagem.Contains("2204") || mensagem.Contains("Diskmotor Overcurrent"))
                return "Motor do disco com sobrecarga.";

            if (mensagem.Contains("2100") || mensagem.Contains("Reject Coin"))
                return "Moeda rejeitada.";

            if (mensagem.Contains("2101") || mensagem.Contains("Inhibited Coin"))
                return "Moeda não aceite.";

            if (mensagem.Contains("2200") || mensagem.Contains("Bill Validator Fail"))
                return "Validação de nota falhou.";

            if (mensagem.Contains("2201") || mensagem.Contains("Transport Problem"))
                return "Problema no transporte da nota.";

            if (mensagem.Contains("2202") || mensagem.Contains("Inhibited"))
                return "Nota não aceite.";

            if (mensagem.Contains("2205") || mensagem.Contains("Fraud Detected"))
                return "Possível fraude detectada.";

            if (mensagem.Contains("2206") || mensagem.Contains("String Fraud"))
                return "Fraude com fio detectada.";

            if (mensagem.Contains("2207") || mensagem.Contains("Refund Not Guaranteed"))
                return "Reembolso não garantido.";

            if (mensagem.Contains("2208") || mensagem.Contains("Vault Full"))
                return "Cofre cheio.";

            // Retornar mensagem original se não reconhecida
            return $"Aviso: {mensagem}";
        }

        /// <summary>
        /// Obtém descrição amigável para códigos de erro de hardware da Alice
        /// </summary>
        private string ObterDescricaoErroHardware(string codigoErro)
        {
            // Códigos de erro da Alice (baseado na documentação oficial)
            switch (codigoErro)
            {
                // Erros de Moedas (2xxx-3xxx)
                case "2100": return "Rejeição de moeda.";
                case "2101": return "Moeda inibida.";
                case "2102": return "Timeout de validação de moeda.";
                case "2103": return "Ciclo de Escrow finalizado.";
                case "2104": return "Diskmotor com sobrecarga.";
                case "2200": return "Falha na validação de nota.";
                case "2201": return "Problema no transporte de nota.";
                case "2202": return "Nota inibida.";
                case "2203": return "Nota puxada para trás.";
                case "2204": return "Nota presa.";
                case "2205": return "Fraude de nota detectada.";
                case "2206": return "Fraude de string de nota detectada.";
                case "2207": return "Reembolso de nota não garantido.";
                case "2208": return "Cofre de notas cheio.";

                // Erros de Sensores e Hardware de Moedas (3100-3112)
                case "3100": return "Timeout do sensor de crédito de moedas.";
                case "3101": return "Erro no 2º fechamento de moedas.";
                case "3102": return "Sensor de crédito de moedas não pronto.";
                case "3103": return "Sensor de crédito de moedas bloqueado.";
                case "3104": return "Porta de aceitação de moedas aberta (não fechada).";
                case "3105": return "Porta de aceitação de moedas fechada (não aberta).";
                case "3106": return "Duplo sinal de saída de moeda.";
                case "3107": return "Disco de moedas travado.";
                case "3108": return "Luz externa de moedas.";
                case "3109": return "Sensor de validação de moedas bloqueado.";
                case "3110": return "Mecanismo de retorno de moedas ativado.";
                case "3111": return "Timeout do Trashcycle de moedas.";
                case "3112": return "Reciclador de moedas inconsistente.";


                // Erros de Hardware de Notas (3200-3209)
                case "3200": return "Transporte de notas preso.";
                case "3201": return "Stacker de notas preso.";
                case "3202": return "Falha no Stacker de notas.";
                case "3203": return "Stacker de notas preso.";
                case "3204": return "Transporte de notas Sm preso.";
                case "3205": return "Transporte de notas Mech preso.";
                case "3206": return "Escrow de notas vazio.";
                case "3207": return "Rota de notas falhou.";
                case "3208": return "Erro de estado de notas.";
                case "3209": return "Falha na dispensa de notas.";

                // Erros de Sistema (3300-3399)
                case "3300": return "Quantidade inesperada recebida.";
                case "3301": return "Erro fatal.";
                case "3303": return "Erro no motor da correia.";
                case "3304": return "Erro no motor classificador.";
                case "3305": return "Erro na válvula RC.";
                case "3399": return "Erro de comunicação.";

                default:
                    return "Erro técnico no hardware.";
            }
        }

        private async Task<bool> CancelarOperacaoAlice(int operationId)
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback =
                        (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(handler))
                    {
                        var authToken = Convert.ToBase64String(
                            Encoding.ASCII.GetBytes($"{ALICE_USER}:{ALICE_PASSWORD}")
                        );
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Basic", authToken);

                        client.Timeout = TimeSpan.FromSeconds(10);

                        var payload = new
                        {
                            operation = new
                            {
                                state = 5
                            }
                        };

                        var json = JsonConvert.SerializeObject(payload);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        // Usar HttpRequestMessage com método PATCH
                        var request = new HttpRequestMessage
                        {
                            Method = new HttpMethod("PATCH"),
                            RequestUri = new Uri($"{ALICE_BASE_URL}/operation?operation_id={operationId}"),
                            Content = content
                        };

                        var response = await client.SendAsync(request);

                        var responseContent = await response.Content.ReadAsStringAsync();

                        System.Diagnostics.Debug.WriteLine($"Cancelamento - Status: {response.StatusCode}");
                        System.Diagnostics.Debug.WriteLine($"Cancelamento - Resposta: {responseContent}");

                        return response.IsSuccessStatusCode;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao cancelar: {ex.Message}");
                return false;
            }
        }

        #region Classes de Dados

        private class AliceResponse
        {
            public int code { get; set; }
            public List<string> messages { get; set; }
            public AliceData data { get; set; }
        }

        private class AliceData
        {
            public AliceOperation operation { get; set; }
        }

        private class AliceOperation
        {
            public int operation_id { get; set; }
            public int state { get; set; }
            public decimal? total_in { get; set; }
            public decimal? total_out { get; set; }
            public int? fail_reason { get; set; }
            public string fail_error { get; set; }
        }

        private class AlicePollingResponse
        {
            public int code { get; set; }
            public List<string> messages { get; set; }
            public AlicePollingData data { get; set; }
        }

        private class AlicePollingData
        {
            public AliceOperation operation { get; set; }
            public List<object> denominations { get; set; }
            public List<object> devices { get; set; }
        }

        private class ResultadoAlice
        {
            public bool Sucesso { get; set; }
            public int OperationId { get; set; }
            public string Mensagem { get; set; }
        }

        #endregion
    }
}