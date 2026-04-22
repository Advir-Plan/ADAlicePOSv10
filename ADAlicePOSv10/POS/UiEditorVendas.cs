using Primavera.Extensibility.POS.Editors;
using Primavera.Extensibility.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Primavera.Extensibility.BusinessEntities.ExtensibilityService.EventArgs;
using ADAlicePOSv10.Licensing;

namespace ADAlicePOSv10.POS
{
    public class UiEditorVendas : EditorVendas
    {
        // Variáveis para guardar as configurações da Alice carregadas da BD
        private string ALICE_BASE_URL = "https://192.168.49.193:8081/api";
        private string ALICE_USER = "b1d0_Admin";
        private string ALICE_PASSWORD = "66aba1";
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
                    double valorTotal = ObterValorTotalDocumento();

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

        private double ObterValorTotalDocumento()
        {
            object documento = this.DocumentoVenda;
            if (documento == null)
            {
                return 0;
            }

            string[] propriedadesPreferenciais =
            {
                "TotalDocumento",
                "TotalDoc",
                "TotalAPagar",
                "TotalFinal",
                "TotalComIva",
                "TotalIliquido",
                "TotalMercIva",
                "TotalMerc"
            };

            foreach (string nomePropriedade in propriedadesPreferenciais)
            {
                try
                {
                    PropertyInfo prop = documento.GetType().GetProperty(nomePropriedade);
                    if (prop == null)
                    {
                        continue;
                    }

                    object valor = prop.GetValue(documento, null);
                    if (valor == null)
                    {
                        continue;
                    }

                    double total = Convert.ToDouble(valor, CultureInfo.InvariantCulture);
                    System.Diagnostics.Debug.WriteLine($"[Alice] Total obtido de DocumentoVenda.{nomePropriedade} = {total}");
                    return total;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Alice] Não foi possível ler DocumentoVenda.{nomePropriedade}: {ex.Message}");
                }
            }

            return 0;
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

                        System.Diagnostics.Debug.WriteLine($"[Alice] POST {ALICE_BASE_URL}/operation/sale");
                        System.Diagnostics.Debug.WriteLine($"[Alice] Payload sale: {json}");

                        var response = await client.PostAsync($"{ALICE_BASE_URL}/operation/sale", content);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        System.Diagnostics.Debug.WriteLine($"[Alice] Resposta sale HTTP {(int)response.StatusCode}: {responseContent}");
                        foreach (var header in response.Headers)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Alice] Header {header.Key}: {string.Join(" | ", header.Value)}");
                        }
                        foreach (var header in response.Content.Headers)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Alice] Content-Header {header.Key}: {string.Join(" | ", header.Value)}");
                        }

                        if (response.IsSuccessStatusCode)
                        {
                            var resultadoResp = JsonConvert.DeserializeObject<AliceResponse>(responseContent);
                            string mensagensApi = ObterMensagensAlice(resultadoResp?.messages);

                            if (resultadoResp != null && resultadoResp.code < 0)
                            {
                                return new ResultadoAlice
                                {
                                    Sucesso = false,
                                    Mensagem = string.IsNullOrWhiteSpace(mensagensApi)
                                        ? $"A Alice devolveu code={resultadoResp.code} ao iniciar a operação."
                                        : $"A Alice devolveu code={resultadoResp.code}: {mensagensApi}"
                                };
                            }

                            int operationId = ExtrairOperationId(response, responseContent, resultadoResp);

                            if (operationId <= 0)
                            {
                                string diagnosticoResposta = ObterDiagnosticoRespostaSale(responseContent);

                                return new ResultadoAlice
                                {
                                    Sucesso = false,
                                    Mensagem = string.IsNullOrWhiteSpace(mensagensApi)
                                        ? $"A Alice aceitou o pedido mas não devolveu um operation_id válido. {diagnosticoResposta}"
                                        : $"A Alice aceitou o pedido mas não devolveu um operation_id válido. Mensagens: {mensagensApi}. {diagnosticoResposta}"
                                };
                            }

                            return new ResultadoAlice
                            {
                                Sucesso = true,
                                OperationId = operationId,
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
            string ultimoDiagnostico = string.Empty;

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

                                        ultimoDiagnostico = ConstruirDiagnosticoOperacao(pollingResponse, operationId);

                                        System.Diagnostics.Debug.WriteLine($">>> Estado: {state}, Valor recebido: {valorRecebido}, Último: {ultimoValorRecebido}");
                                        if (!string.IsNullOrWhiteSpace(ultimoDiagnostico) && tentativas % 10 == 0)
                                        {
                                            System.Diagnostics.Debug.WriteLine($">>> Diagnóstico: {ultimoDiagnostico}");
                                        }

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
                                        else if (state == 8)
                                        {
                                            formPagamento?.MostrarErro("A operação já não existe na Alice (state 8 - Not Found).");
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

                        string mensagemTimeout = "Tempo limite excedido.";
                        if (!string.IsNullOrWhiteSpace(ultimoDiagnostico))
                        {
                            mensagemTimeout += $"\n\nÚltimo diagnóstico da Alice:\n{ultimoDiagnostico}";
                        }

                        formPagamento?.MostrarErro(mensagemTimeout);
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
            // Baseado na documentação atual da API Alice/4Cash
            // fail_reason valores:
            // 0 = None
            // 1 = No change or change/refund deliver failed
            // 2 = Timeout
            // 3 = All recyclers in error
            // 4 = Database inconsistency found
            // 5 = Front door open
            // 6 = Error occurred (check fail_error)
            // 7 = The machine was shut down before finishing the current operation
            // 255 = Critical timeout

            switch (failReason)
            {
                case 0:
                    return "A operação falhou sem detalhe adicional da Alice.";

                case 1:
                    return "Troco insuficiente ou falha na devolução do troco/reembolso.\n\nVerifique a disponibilidade do moedeiro.";

                case 2:
                    return "Tempo limite excedido.\n\nA operação demorou demasiado tempo a concluir.";

                case 3:
                    return "Todos os recicladores estão em erro.\n\nO equipamento não consegue processar dinheiro neste momento.";

                case 4:
                    return "Foi detetada uma inconsistência interna na Alice.\n\nÉ necessária verificação técnica.";

                case 5:
                    return "A porta frontal do equipamento está aberta.\n\nFeche a porta e tente novamente.";

                case 6:
                    string codigoErro = !string.IsNullOrEmpty(failError) ? failError : "Desconhecido";
                    string descricaoErro = ObterDescricaoErroHardware(codigoErro);
                    return $"Erro no terminal de pagamento.\n\n{descricaoErro}\n\nCódigo: {codigoErro}\n\nContacte o suporte técnico.";

                case 7:
                    return "A máquina foi desligada antes de concluir a operação.";

                case 255:
                    return "Timeout crítico da Alice.\n\nRecomendado contactar integrations@4cash.pt.";

                default:
                    return $"Erro no pagamento.\n\nFail reason: {failReason ?? -1}\nFail error: {failError ?? "n/d"}";
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
                // Erros de Sensores e Hardware de Moedas (3100-3112)
                case "3100": return "Uma moeda válida não chegou ao sensor de crédito dentro do tempo esperado.";
                case "3101": return "Foram detetadas duas moedas no mesmo canal; ambas serão rejeitadas.";
                case "3102": return "A moeda foi rejeitada porque o sensor de crédito está ocupado ou bloqueado.";
                case "3103": return "Sensor de crédito de moedas bloqueado.";
                case "3104": return "Foi aceite uma moeda desconhecida ou inibida.";
                case "3105": return "Uma moeda válida foi rejeitada.";
                case "3106": return "Uma moeda está a gerar múltiplos sinais no sensor de saída.";
                case "3107": return "O bloqueio não foi resolvido e o motor do disco foi desligado.";
                case "3108": return "Foi detetada luz interferente no aceitador de moedas.";
                case "3109": return "Moeda presa na validação ou sensor de moedas a precisar de limpeza.";
                case "3110": return "Mecanismo de retorno de moedas ativado.";
                case "3111": return "Timeout no ciclo de rejeição; a porta de descarte pode estar bloqueada.";


                // Erros de Hardware de Notas (3200-3208)
                case "3200": return "Nota presa no transporte.";
                case "3201": return "Stacker de notas preso.";
                case "3202": return "Falha no Stacker de notas.";
                case "3203": return "Stacker de notas preso.";
                case "3204": return "Nota presa no transporte em modo seguro.";
                case "3205": return "Falha no mecanismo anti-string.";
                case "3208": return "Aceitador de notas em estado de erro.";

                // Erros de Sistema (3300-3399)
                case "3300": return "Reciclador sem quantidade disponível ou com quantidade inexistente.";
                case "3301": return "Reciclador sem resposta.";
                case "3302": return "Aceitador de moedas/notas desativado ou com erro fatal.";
                case "3303": return "Erro no motor da correia.";
                case "3304": return "Erro no motor do classificador.";
                case "3305": return "Saída do reciclador bloqueada ou obstruída.";
                case "3399": return "Erro no protocolo de comunicação com o equipamento.";

                default:
                    return "Erro técnico no hardware ou comunicação com a Alice.";
            }
        }

        private string ObterMensagensAlice(List<string> mensagens)
        {
            if (mensagens == null || mensagens.Count == 0)
            {
                return string.Empty;
            }

            var mensagensValidas = mensagens.FindAll(m => !string.IsNullOrWhiteSpace(m));
            return mensagensValidas.Count == 0
                ? string.Empty
                : string.Join(" | ", mensagensValidas);
        }

        private string ConstruirDiagnosticoOperacao(AlicePollingResponse pollingResponse, int operationId)
        {
            var partes = new List<string>();

            string mensagens = ObterMensagensAlice(pollingResponse?.messages);
            if (!string.IsNullOrWhiteSpace(mensagens))
            {
                partes.Add($"messages={mensagens}");
            }

            if (pollingResponse?.data?.devices != null && pollingResponse.data.devices.Count > 0)
            {
                var devicesCompactos = new List<string>();
                foreach (var device in pollingResponse.data.devices)
                {
                    if (device == null)
                    {
                        continue;
                    }

                    string texto = device.ToString(Formatting.None);
                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        devicesCompactos.Add(texto);
                    }
                }

                if (devicesCompactos.Count > 0)
                {
                    partes.Add($"devices={string.Join(" | ", devicesCompactos)}");
                }
            }

            if (pollingResponse?.data?.denominations != null && pollingResponse.data.denominations.Count > 0)
            {
                var denominacoes = new List<string>();
                foreach (var denomination in pollingResponse.data.denominations)
                {
                    if (denomination == null)
                    {
                        continue;
                    }

                    string texto = denomination.ToString(Formatting.None);
                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        denominacoes.Add(texto);
                    }
                }

                if (denominacoes.Count > 0)
                {
                    partes.Add($"denominations={string.Join(" | ", denominacoes)}");
                }
            }

            if (partes.Count == 0)
            {
                return $"operation_id={operationId}, sem mensagens adicionais";
            }

            return string.Join("\n", partes);
        }

        private int ExtrairOperationId(HttpResponseMessage response, string responseContent, AliceResponse respostaTipada)
        {
            if (respostaTipada?.data?.operation?.operation_id > 0)
            {
                return respostaTipada.data.operation.operation_id;
            }

            int operationIdDosHeaders = ExtrairOperationIdDosHeaders(response);
            if (operationIdDosHeaders > 0)
            {
                return operationIdDosHeaders;
            }

            try
            {
                var json = JObject.Parse(responseContent);
                int operationIdDoJson = ExtrairOperationIdDeToken(json);
                if (operationIdDoJson > 0)
                {
                    return operationIdDoJson;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Alice] Não foi possível interpretar a resposta do sale como JSON: {ex.Message}");
            }

            int operationIdDoTexto = ExtrairOperationIdDoTexto(responseContent);
            if (operationIdDoTexto > 0)
            {
                return operationIdDoTexto;
            }

            return 0;
        }

        private string ObterDiagnosticoRespostaSale(string responseContent)
        {
            try
            {
                var json = JObject.Parse(responseContent);
                var partes = new List<string>();

                if (json["data"] is JObject dataObj)
                {
                    partes.Add($"data keys: {string.Join(", ", ObterNomesPropriedades(dataObj))}");
                }

                if (json["operation"] is JObject operationObj)
                {
                    partes.Add($"operation keys: {string.Join(", ", ObterNomesPropriedades(operationObj))}");
                }

                if (json["data"]?["operation"] is JObject dataOperationObj)
                {
                    partes.Add($"data.operation keys: {string.Join(", ", ObterNomesPropriedades(dataOperationObj))}");
                }

                if (partes.Count > 0)
                {
                    return string.Join(" ", partes);
                }
            }
            catch
            {
                // Se não for JSON válido, devolvemos um resumo bruto abaixo.
            }

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return "A resposta veio vazia.";
            }

            string resumo = responseContent.Length > 300
                ? responseContent.Substring(0, 300) + "..."
                : responseContent;

            return $"Resposta: {resumo}";
        }

        private int ExtrairOperationIdDosHeaders(HttpResponseMessage response)
        {
            if (response == null)
            {
                return 0;
            }

            foreach (var header in response.Headers)
            {
                foreach (var valor in header.Value)
                {
                    int operationId = ExtrairOperationIdDoTexto(valor);
                    if (operationId > 0)
                    {
                        return operationId;
                    }
                }
            }

            foreach (var header in response.Content.Headers)
            {
                foreach (var valor in header.Value)
                {
                    int operationId = ExtrairOperationIdDoTexto(valor);
                    if (operationId > 0)
                    {
                        return operationId;
                    }
                }
            }

            if (response.Headers.Location != null)
            {
                int operationId = ExtrairOperationIdDoTexto(response.Headers.Location.ToString());
                if (operationId > 0)
                {
                    return operationId;
                }
            }

            return 0;
        }

        private int ExtrairOperationIdDeToken(JToken token)
        {
            if (token == null)
            {
                return 0;
            }

            if (token.Type == JTokenType.Object)
            {
                foreach (var child in token.Children<JProperty>())
                {
                    if (string.Equals(child.Name, "operation_id", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(child.Name, "operationId", StringComparison.OrdinalIgnoreCase) ||
                        (string.Equals(child.Name, "id", StringComparison.OrdinalIgnoreCase) && child.Parent?.Parent?["operation"] != null))
                    {
                        int valorDireto = ConverterTokenEmInt(child.Value);
                        if (valorDireto > 0)
                        {
                            return valorDireto;
                        }
                    }

                    int valorFilho = ExtrairOperationIdDeToken(child.Value);
                    if (valorFilho > 0)
                    {
                        return valorFilho;
                    }
                }
            }

            if (token.Type == JTokenType.Array)
            {
                foreach (var item in token.Children())
                {
                    int valorItem = ExtrairOperationIdDeToken(item);
                    if (valorItem > 0)
                    {
                        return valorItem;
                    }
                }
            }

            return 0;
        }

        private int ConverterTokenEmInt(JToken token)
        {
            if (token == null)
            {
                return 0;
            }

            if (token.Type == JTokenType.Integer)
            {
                int valor = token.Value<int>();
                return valor > 0 ? valor : 0;
            }

            if (token.Type == JTokenType.String)
            {
                int valor;
                if (int.TryParse(token.Value<string>(), out valor) && valor > 0)
                {
                    return valor;
                }
            }

            return 0;
        }

        private int ExtrairOperationIdDoTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return 0;
            }

            var padroes = new[]
            {
                @"operation_id\s*[""=:]\s*[""]?(?<id>\d+)",
                @"operationId\s*[""=:]\s*[""]?(?<id>\d+)",
                @"[?&]operation_id=(?<id>\d+)",
                @"\boperation\b.?\bid\b\s*[""=:]\s*[""]?(?<id>\d+)"
            };

            foreach (var padrao in padroes)
            {
                var match = Regex.Match(texto, padrao, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int valor;
                    if (int.TryParse(match.Groups["id"].Value, out valor) && valor > 0)
                    {
                        return valor;
                    }
                }
            }

            return 0;
        }

        private IEnumerable<string> ObterNomesPropriedades(JObject obj)
        {
            foreach (var prop in obj.Properties())
            {
                yield return prop.Name;
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
            public List<JToken> denominations { get; set; }
            public List<JToken> devices { get; set; }
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
