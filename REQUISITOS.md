# Requisitos do Sistema ADAlice POS v10

## Requisitos Obrigatórios para Integração com ERP PRIMAVERA

---

## 1. ERP PRIMAVERA

### Versão Obrigatória
- **PRIMAVERA SG100** (versão 100)
- **Caminho de instalação:** `C:\Program Files\PRIMAVERA\SG100\`
- **Módulo POS PRIMAVERA** instalado e ativo

### DLLs PRIMAVERA Necessárias
As seguintes DLLs do PRIMAVERA devem estar presentes em `C:\Program Files\PRIMAVERA\SG100\APL\`:

- `ErpBS100.dll` - Serviços de negócio ERP
- `Primavera.Extensibility.Attributes.dll` - Definições de atributos
- `Primavera.Extensibility.BusinessEntities.dll` - Entidades de negócio
- `Primavera.Extensibility.CustomCode.dll` - Suporte a código customizado
- `Primavera.Extensibility.CustomForm.dll` - Suporte a formulários customizados
- `Primavera.Extensibility.Integration.dll` - Serviços de integração
- `Primavera.Extensibility.POS.dll` - **Módulo POS (CRÍTICO)**
- `StdBE100.dll` - Entidades de negócio padrão
- `StdPlatBS100.dll` - Serviços de plataforma
- `VndBE100.dll` - Entidades de negócio de vendas

---

## 2. FRAMEWORK E AMBIENTE

### Obrigatório
- **.NET Framework 4.7.2** ou superior
- **Sistema Operativo:** Windows (7/8/10/11 ou Windows Server)
- **Arquitetura:** AnyCPU (suporta x86 e x64)

### Permissões Necessárias
- Acesso de **leitura** à pasta `C:\Program Files\PRIMAVERA\SG100\APL\`
- Acesso de **escrita** para instalação do DLL e licença
- Acesso **WMI** (Windows Management Instrumentation) para geração de Hardware ID

---

## 3. TERMINAL DE PAGAMENTO ALICE

### Requisitos de Rede
- **Conectividade HTTPS** ao terminal de pagamento ALICE
- **IP do Terminal:** 192.168.1.83 (configurável)
- **Porta:** 8081
- **Protocolo:** HTTPS com certificado auto-assinado aceite
- **Latência de Rede:** Deve suportar polling a cada 500ms
- **Timeout:** Janela de 5 minutos para conclusão de pagamento

### Configuração da API ALICE
- **Endpoint Base:** `https://192.168.1.83:8081/api`
- **Autenticação:** Basic Auth (credenciais Base64)
- **Credenciais Padrão:**
  - Utilizador: `8957_Admin`
  - Password: `3603ee`
  - ⚠️ **NOTA:** Estas credenciais devem ser alteradas antes de produção

### Endpoints Utilizados
1. `POST /api/operation/sale` - Iniciar operação de pagamento
2. `GET /api/operation?operation_id={id}` - Monitorizar estado da operação
3. `PATCH /api/operation?operation_id={id}` - Cancelar operação

---

## 4. SISTEMA DE LICENCIAMENTO

### Ficheiro de Licença Obrigatório
- **Nome do Ficheiro:** `ADAlicePOS.lic`
- **Localização:** `C:\Program Files\PRIMAVERA\SG100\ADAlicePOS.lic`
- **Formato:** Encriptado AES-256, codificado em Base64
- **Vinculação:** Ligado ao Hardware ID da máquina

### Componentes do Hardware ID
O Hardware ID é calculado com base em:
- **CPU Processor ID** (via WMI)
- **BIOS Serial Number** (via WMI)
- **Motherboard Serial Number** (via WMI)
- Hash SHA-256 de 32 caracteres

### Geração de Licença
- Utilizar a ferramenta **`LicenseGenerator.exe`** incluída no projeto
- Modos: Interativo ou linha de comandos
- Suporta: obtenção de Hardware ID, geração e validação de licenças

### Conteúdo da Licença
- Hardware ID (32 caracteres)
- Data de Expiração
- Nome da Empresa
- Data de Emissão

---

## 5. DEPENDÊNCIAS DE SOFTWARE

### Pacotes NuGet Obrigatórios
- **Newtonsoft.Json 13.0.4** - Serialização/Deserialização JSON para API de pagamentos

### Referências do Sistema .NET
- `System.Net.Http` - Chamadas API HTTPS
- `System.Management` - Obtenção de Hardware ID via WMI
- `System.Security.Cryptography` - AES-256 e SHA-256
- `System.Windows.Forms` - Interface do diálogo de pagamento
- `System.Drawing` - Gráficos no formulário de pagamento

---

## 6. ESTRUTURA DE INSTALAÇÃO

### Ficheiros a Instalar

1. **DLL Principal:**
   - `ADAlicePOSv10.dll`
   - **Destino:** `C:\Program Files\PRIMAVERA\SG100\APL\`

2. **Ficheiro de Licença:**
   - `ADAlicePOS.lic`
   - **Destino:** `C:\Program Files\PRIMAVERA\SG100\`

### Passos de Instalação

1. ✅ Compilar `ADAlicePOSv10.dll` em modo Release
2. ✅ Copiar DLL para `C:\Program Files\PRIMAVERA\SG100\APL\`
3. ✅ Gerar licença com `LicenseGenerator.exe`
4. ✅ Colocar `ADAlicePOS.lic` em `C:\Program Files\PRIMAVERA\SG100\`
5. ✅ Configurar conectividade de rede ao terminal ALICE (IP: 192.168.1.83)
6. ✅ Reiniciar ERP PRIMAVERA
7. ✅ Testar fluxo de pagamento

---

## 7. REQUISITOS DE REDE

### Conectividade Obrigatória

#### Servidor PRIMAVERA
- Acesso local ou em rede ao PRIMAVERA SG100

#### Terminal ALICE
- Conectividade de rede ao terminal em `192.168.1.83:8081`
- Suporte a HTTPS
- Permitir certificados SSL auto-assinados
- Latência de rede que suporte polling a cada 500ms
- Janela de timeout de 5 minutos para conclusão de pagamento

### Configuração de Firewall
- **Porta 8081** deve estar acessível para tráfego HTTPS de saída
- Se o terminal ALICE estiver noutra sub-rede, configurar roteamento adequado

---

## 8. INTEGRAÇÃO COM PRIMAVERA

### Modelo de Extensibilidade
O sistema estende funcionalidade POS do PRIMAVERA através de classes editoras:

```csharp
// Extensão do Editor POS principal
public class UiEditorVendas : EditorVendas

// Extensão do Editor de Inventários
public class PriClass1 : EditorInventarios
```

### Pontos de Integração
- **Hook de Evento:** `DepoisDeConfirmar()` - Executa após confirmação de venda
- **Entidade de Negócio:** Acesso direto a `DocumentoVenda` (documento de venda)
- **Ciclo de Vida:** Validação de licença no construtor (uma validação por sessão)

### Acesso a Dados
- **Leitura:** Dados do documento de venda (total, cliente, modo de pagamento)
- **Escrita:** Implícita através do workflow de confirmação de documentos do PRIMAVERA
- **Sem Base de Dados Direta:** Toda a gestão de dados passa pelas entidades de negócio do PRIMAVERA

---

## 9. INTERFACE DE UTILIZADOR

### Diálogo de Pagamento (`FormPagamentoAlice`)
- Formulário Windows Forms customizado
- Exibe progresso de pagamento em tempo real
- Mostra valor recebido vs. valor em dívida
- Calcula troco/reembolso
- Suporta cancelamento pelo utilizador com reembolso automático
- Estados: Aguardando, Processando, Sucesso, Cancelado, Falhado

---

## 10. CONSIDERAÇÕES DE SEGURANÇA

### ⚠️ Antes de Produção - OBRIGATÓRIO

#### Credenciais Hardcoded
- As credenciais da API ALICE estão no código fonte:
  - Utilizador: `8957_Admin`
  - Password: `3603ee`
- **AÇÃO NECESSÁRIA:** Mover para ficheiros de configuração seguros ou vault

#### Master Key de Encriptação
- MASTER_KEY está embebida no código
- **AÇÃO NECESSÁRIA:** Alterar antes de qualquer deployment em produção
- Todas as licenças terão de ser regeneradas se a chave for comprometida
- **Localização no código:**
  - `LicenseManager.cs`
  - `Program.cs` (LicenseGenerator)

#### Validação de Certificados SSL
- Atualmente desativada para aceitar certificados auto-assinados
- **Recomendação:** Implementar validação adequada em produção

#### Segurança de Licença
- Hardware ID vinculado à máquina específica
- Previne portabilidade de licença
- Validação offline (sem chamadas a servidor)
- Encriptação AES-256 com derivação de chave PBKDF2

---

## 11. CHECKLIST DE PRÉ-INSTALAÇÃO

### Infraestrutura
- [ ] PRIMAVERA SG100 instalado com diretório APL acessível
- [ ] .NET Framework 4.7.2 instalado no servidor/workstation
- [ ] Conectividade de rede ao terminal ALICE (192.168.1.83:8081)
- [ ] Conta de utilizador Windows com acesso WMI

### Configuração
- [ ] Alterar MASTER_KEY em `LicenseManager.cs` e `Program.cs`
- [ ] Atualizar ALICE_BASE_URL, ALICE_USER, ALICE_PASSWORD com valores reais
- [ ] Atualizar IP do terminal ALICE se diferente de 192.168.1.83
- [ ] Gerar licença usando `LicenseGenerator.exe` com Hardware ID correto

### Deployment
- [ ] Compilar `ADAlicePOSv10.dll` em modo Release
- [ ] Copiar DLL para `C:\Program Files\PRIMAVERA\SG100\APL\`
- [ ] Colocar `ADAlicePOS.lic` em `C:\Program Files\PRIMAVERA\SG100\`
- [ ] Verificar conectividade de rede ao terminal ALICE
- [ ] Testar fluxo de pagamento com ERP PRIMAVERA

### Teste de Conectividade
Testar conectividade ao terminal ALICE:
```bash
# Teste de ping
ping 192.168.1.83

# Teste de porta HTTPS (PowerShell)
Test-NetConnection -ComputerName 192.168.1.83 -Port 8081
```

---

## 12. RESOLUÇÃO DE PROBLEMAS COMUNS

### Licença Inválida
- Verificar se `ADAlicePOS.lic` está em `C:\Program Files\PRIMAVERA\SG100\`
- Confirmar que o Hardware ID corresponde à máquina atual
- Verificar se a licença não expirou

### Falha na Ligação ao Terminal ALICE
- Verificar conectividade de rede (ping, Test-NetConnection)
- Confirmar que o terminal está ligado e acessível
- Verificar firewall e regras de rede
- Confirmar IP e porta corretos no código

### DLLs do PRIMAVERA Não Encontradas
- Verificar instalação do PRIMAVERA SG100
- Confirmar que o módulo POS está instalado
- Verificar caminhos das referências no projeto

### Erro de Permissões
- Executar PRIMAVERA como Administrador
- Verificar permissões de escrita em `C:\Program Files\`
- Confirmar acesso WMI para geração de Hardware ID

---

## 13. INFORMAÇÕES DO PROJETO

### Metadata da Assembly
```
Título: ADAlicePOSv10
Empresa: PRIMAVERA Business Software Solutions, SA
Produto: PRIMAVERA DEVELOPERS NETWORK
Versão: 1.0.0.0 (File version: 1.0001.0000.0012)
```

### Ficheiros de Referência
- [`/ADAlicePOSv10/ADAlicePOSv10.csproj`](ADAlicePOSv10/ADAlicePOSv10.csproj) - Configuração do projeto
- [`/ADAlicePOSv10/POS/UiEditorVendas.cs`](ADAlicePOSv10/POS/UiEditorVendas.cs) - Integração POS principal e API ALICE
- [`/ADAlicePOSv10/POS/PriClass1.cs`](ADAlicePOSv10/POS/PriClass1.cs) - Extensão de inventários
- [`/ADAlicePOSv10/POS/FormPagamentoAlice.cs`](ADAlicePOSv10/POS/FormPagamentoAlice.cs) - Interface de pagamento
- [`/ADAlicePOSv10/Licensing/LicenseManager.cs`](ADAlicePOSv10/Licensing/LicenseManager.cs) - Motor de validação de licença
- [`/LicenseGenerator/Program.cs`](LicenseGenerator/Program.cs) - Ferramenta de geração de licenças
- [`/LICENCIAMENTO.md`](LICENCIAMENTO.md) - Documentação abrangente de licenciamento

---

## 14. SUPORTE E CONTACTOS

Para questões técnicas ou suporte:
- Consultar documentação adicional em [`LICENCIAMENTO.md`](LICENCIAMENTO.md)
- Contactar PRIMAVERA Business Software Solutions, SA

---

## 15. RESUMO EXECUTIVO

### Requisitos Mínimos Obrigatórios

| Componente | Requisito |
|------------|-----------|
| **ERP** | PRIMAVERA SG100 com módulo POS |
| **Framework** | .NET Framework 4.7.2 ou superior |
| **Sistema Operativo** | Windows (7/8/10/11 ou Server) |
| **Terminal Pagamento** | ALICE com API acessível em rede |
| **Licença** | Ficheiro `ADAlicePOS.lic` válido |
| **Rede** | Conectividade HTTPS ao terminal (porta 8081) |
| **Permissões** | Acesso WMI e escrita em `C:\Program Files\` |

### Tempo Estimado de Instalação
- Instalação básica: 15-30 minutos
- Configuração de rede e testes: 30-60 minutos
- Geração e instalação de licença: 10-15 minutos
- **Total:** 1-2 horas (primeira instalação)

---

**Documento gerado:** 2026-01-08
**Versão do sistema:** ADAlice POS v10 (1.0.0.0)
**Compatível com:** PRIMAVERA SG100
