# Sistema de Licenciamento - ADAlice POS v10

## 📋 Visão Geral

Este sistema protege o ADAlice POS contra uso não autorizado através de licenças vinculadas ao hardware do computador.

### ✅ Características

- **Validação única na inicialização**: A licença é verificada apenas 1x quando o PRIMAVERA carrega o módulo
- **Baseado em Hardware ID**: Cada licença é vinculada a um computador específico
- **Criptografia AES-256**: As licenças são protegidas com criptografia forte
- **Sem servidor online**: Funciona completamente offline
- **Data de expiração**: Controle de validade por período

---

## 🔧 Como Funciona

### 1. Hardware ID

O sistema gera um identificador único do computador baseado em:
- **CPU ID** (Identificador do processador)
- **BIOS Serial** (Número de série da BIOS)
- **Motherboard Serial** (Número de série da placa-mãe)

Estes dados são combinados e convertidos em um hash SHA-256 de 32 caracteres.

### 2. Validação

Quando o PRIMAVERA ERP inicia e carrega o módulo ADAlice POS:

```
1. Construtor UiEditorVendas() é chamado
2. LicenseManager.ValidateLicense() executa
3. Verifica se existe arquivo "ADAlicePOS.lic"
4. Descriptografa e valida:
   - Hardware ID corresponde?
   - Licença está expirada?
5. Se válida: módulo carrega normalmente
6. Se inválida: mostra erro e impede carregamento
```

**Importante**: A validação só ocorre **UMA vez por sessão**, não a cada venda!

---

## 📦 Gerando Licenças

### Passo 1: Compilar o Gerador de Licenças

1. Abra o Visual Studio
2. Compile o projeto **LicenseGenerator** em modo **Release**
3. O executável estará em: `LicenseGenerator\bin\Release\LicenseGenerator.exe`

### Passo 2: Obter Hardware ID do Cliente

**Opção A - Remotamente:**
Peça ao cliente para executar no computador dele:

```cmd
LicenseGenerator.exe
```

Selecione opção **[1] Obter Hardware ID desta máquina**

O cliente deve enviar o Hardware ID exibido (32 caracteres).

**Opção B - Presencialmente:**
Execute o gerador diretamente no computador do cliente.

### Passo 3: Gerar a Licença

Execute o gerador em modo interativo:

```cmd
LicenseGenerator.exe
```

Selecione opção **[2] Gerar licença**:

```
Hardware ID do cliente: ABC123...XYZ789
Validade em dias: 365
Nome da empresa: Restaurante Exemplo Lda
```

**Modo linha de comando (alternativa):**

```cmd
LicenseGenerator.exe ABC123...XYZ789 365 "Restaurante Exemplo"
```

### Passo 4: Entregar ao Cliente

O gerador cria um arquivo `.lic`, exemplo:
```
ADAlicePOS_Restaurante_Exemplo_Lda_20260106.lic
```

**Instruções para o cliente:**

1. Copie o arquivo `.lic` para: `C:\Program Files\PRIMAVERA\SG100\`
2. Renomeie para: **ADAlicePOS.lic** (exatamente este nome!)
3. Reinicie o PRIMAVERA ERP

---

## 🛠️ Validação de Licenças

Para verificar se uma licença está válida:

```cmd
LicenseGenerator.exe
```

Selecione opção **[3] Validar licença existente**

Informe o caminho do arquivo `.lic`

O gerador mostrará:
- Hardware ID
- Nome da empresa
- Data de emissão
- Data de expiração
- Status (válida/expirada)

---

## ⚠️ Mensagens de Erro

### "Licença não encontrada!"

**Causa**: Arquivo `ADAlicePOS.lic` não existe na pasta do PRIMAVERA

**Solução**:
1. Verifique se o arquivo está em: `C:\Program Files\PRIMAVERA\SG100\`
2. Confirme que o nome é exatamente: `ADAlicePOS.lic` (sem número no final)

---

### "Licença inválida para este computador!"

**Causa**: O Hardware ID da licença não corresponde ao computador atual

**Possíveis razões**:
- Cliente tentou usar licença de outro computador
- Hardware foi alterado (troca de placa-mãe, CPU, etc.)
- Arquivo foi editado/corrompido

**Solução**:
1. Obtenha o novo Hardware ID do computador
2. Gere uma nova licença

---

### "Licença expirada!"

**Causa**: A data de validade da licença foi ultrapassada

**Solução**:
1. Gere uma nova licença com nova data de expiração
2. Entregue ao cliente

---

## 🔐 Segurança

### IMPORTANTE - Proteja a Chave Mestra!

O arquivo `LicenseManager.cs` (linha 11) e `Program.cs` do gerador (linha 13) contêm:

```csharp
private const string MASTER_KEY = "ADAlice_POS_v10_MasterKey_2026_SecureHash";
```

**ALTERE ESTA CHAVE antes de distribuir!**

⚠️ **Cuidados:**
- Use uma chave complexa e única
- **NUNCA** compartilhe esta chave
- **NUNCA** distribua o LicenseGenerator.exe aos clientes
- Mantenha o código-fonte do gerador em local seguro
- Se a chave vazar, todas as licenças devem ser regeneradas com nova chave

---

## 📁 Estrutura de Arquivos

```
ADAlicePOSv10/
├── ADAlicePOSv10/
│   ├── Licensing/
│   │   └── LicenseManager.cs        ← Validador de licenças
│   └── POS/
│       └── UiEditorVendas.cs        ← Validação no construtor
│
└── LicenseGenerator/
    ├── Program.cs                    ← Gerador de licenças
    └── LicenseGenerator.exe          ← Executável (após build)
```

---

## 🧪 Testando o Sistema

### Teste 1: Sem Licença

1. Compile o projeto ADAlicePOSv10
2. Copie a DLL para a pasta do PRIMAVERA (sem arquivo .lic)
3. Inicie o PRIMAVERA
4. **Resultado esperado**: Mensagem de erro "Licença não encontrada!"

### Teste 2: Licença Válida

1. Execute: `LicenseGenerator.exe`
2. Opção [1] para obter Hardware ID
3. Opção [2] para gerar licença (365 dias)
4. Copie o arquivo `.lic` gerado para: `C:\Program Files\PRIMAVERA\SG100\ADAlicePOS.lic`
5. Inicie o PRIMAVERA
6. **Resultado esperado**: Módulo carrega normalmente

### Teste 3: Licença Expirada

1. Gere uma licença com **-1 dias** (já expirada)
2. Coloque na pasta do PRIMAVERA
3. Inicie o PRIMAVERA
4. **Resultado esperado**: Mensagem "Licença expirada!"

### Teste 4: Hardware Diferente

1. Gere licença com Hardware ID fictício: `AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA`
2. Coloque na pasta do PRIMAVERA
3. Inicie o PRIMAVERA
4. **Resultado esperado**: "Licença inválida para este computador!"

---

## 💡 Dicas

### Renovação de Licenças

Para renovar uma licença expirada:
1. Use o **mesmo Hardware ID** do cliente
2. Gere nova licença com nova data de expiração
3. Cliente apenas substitui o arquivo `.lic`

### Backup de Licenças

Recomende aos clientes:
- Fazer backup do arquivo `ADAlicePOS.lic`
- Guardar junto com backups do PRIMAVERA
- Em caso de reinstalação, basta copiar o arquivo de volta

### Migração de Hardware

Se o cliente trocar de computador:
1. Cliente informa novo Hardware ID
2. Você gera nova licença para o novo hardware
3. Antiga licença fica automaticamente inválida no novo PC

### Licenças Perpétuas

Para licenças sem expiração, use um valor alto:

```cmd
LicenseGenerator.exe ABC123... 36500 "Cliente"
```

(36500 dias = 100 anos)

---

## 📞 Suporte

Em caso de problemas:

1. Verifique os logs do PRIMAVERA
2. Use o gerador para validar o arquivo `.lic`
3. Confirme que as versões da DLL e da licença são compatíveis
4. Em último caso, gere uma nova licença

---

## 📄 Changelog

### v1.0 (2026-01-06)
- ✅ Sistema de licenciamento baseado em Hardware ID
- ✅ Validação única na inicialização
- ✅ Criptografia AES-256
- ✅ Gerador de licenças interativo
- ✅ Suporte a datas de expiração
