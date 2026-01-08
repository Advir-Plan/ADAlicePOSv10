using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace ADAlicePOSv10.LicenseGenerator
{
    /// <summary>
    /// Aplicação console para gerar licenças para o ADAlice POS
    /// USO: LicenseGenerator.exe [hardwareId] [dias] [nomeEmpresa]
    /// </summary>
    class Program
    {
        private const string MASTER_KEY = "ADAlice_POS_v10_MasterKey_2026_SecureHash"; // DEVE ser igual ao LicenseManager!

        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║      ADAlice POS v10 - Gerador de Licenças              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            try
            {
                // Modo interativo
                if (args.Length == 0)
                {
                    RunInteractiveMode();
                }
                // Modo linha de comando
                else if (args.Length == 3)
                {
                    string hardwareId = args[0];
                    int dias = int.Parse(args[1]);
                    string nomeEmpresa = args[2];

                    GenerateAndSaveLicense(hardwareId, dias, nomeEmpresa);
                }
                else
                {
                    ShowUsage();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERRO] {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nPressione qualquer tecla para sair...");
                Console.ReadKey();
            }
        }

        static void RunInteractiveMode()
        {
            Console.WriteLine("Escolha uma opção:");
            Console.WriteLine("  [1] Obter Hardware ID desta máquina");
            Console.WriteLine("  [2] Gerar licença com Hardware ID específico");
            Console.WriteLine("  [3] Validar licença existente");
            Console.WriteLine();
            Console.Write("Opção: ");

            string opcao = Console.ReadLine();
            Console.WriteLine();

            switch (opcao)
            {
                case "1":
                    ShowCurrentHardwareId();
                    break;

                case "2":
                    GenerateLicenseInteractive();
                    break;

                case "3":
                    ValidateLicenseInteractive();
                    break;

                default:
                    Console.WriteLine("Opção inválida!");
                    break;
            }

            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        static void ShowCurrentHardwareId()
        {
            Console.WriteLine("═══ Hardware ID desta Máquina ═══");
            Console.WriteLine();

            try
            {
                string hardwareId = GetHardwareId();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Hardware ID: {hardwareId}");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Use este Hardware ID para gerar uma licença para este computador.");
                Console.WriteLine();

                // Detalhes técnicos
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Detalhes técnicos:");
                Console.WriteLine($"  CPU ID:        {GetComponentId("Win32_Processor", "ProcessorId")}");
                Console.WriteLine($"  BIOS Serial:   {GetComponentId("Win32_BIOS", "SerialNumber")}");
                Console.WriteLine($"  Board Serial:  {GetComponentId("Win32_BaseBoard", "SerialNumber")}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erro ao obter Hardware ID: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void GenerateLicenseInteractive()
        {
            Console.WriteLine("═══ Gerar Nova Licença ═══");
            Console.WriteLine();

            Console.Write("Hardware ID do cliente: ");
            string hardwareId = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(hardwareId) || hardwareId.Length != 32)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Hardware ID inválido! Deve ter 32 caracteres.");
                Console.ResetColor();
                return;
            }

            Console.Write("Validade em dias (ex: 365 para 1 ano): ");
            if (!int.TryParse(Console.ReadLine(), out int dias) || dias <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Número de dias inválido!");
                Console.ResetColor();
                return;
            }

            Console.Write("Nome da empresa: ");
            string nomeEmpresa = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(nomeEmpresa))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Nome da empresa não pode estar vazio!");
                Console.ResetColor();
                return;
            }

            Console.WriteLine();
            GenerateAndSaveLicense(hardwareId, dias, nomeEmpresa);
        }

        static void GenerateAndSaveLicense(string hardwareId, int dias, string nomeEmpresa)
        {
            DateTime expirationDate = DateTime.Now.AddDays(dias);

            Console.WriteLine("═══ Gerando Licença ═══");
            Console.WriteLine();
            Console.WriteLine($"Hardware ID:  {hardwareId}");
            Console.WriteLine($"Empresa:      {nomeEmpresa}");
            Console.WriteLine($"Emissão:      {DateTime.Now:dd/MM/yyyy}");
            Console.WriteLine($"Expiração:    {expirationDate:dd/MM/yyyy} ({dias} dias)");
            Console.WriteLine();

            try
            {
                string license = GenerateLicense(hardwareId, expirationDate, nomeEmpresa);

                // Salvar no arquivo
                string fileName = $"ADAlicePOS_{nomeEmpresa.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.lic";
                File.WriteAllText(fileName, license);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Licença gerada com sucesso!");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine($"Arquivo: {Path.GetFullPath(fileName)}");
                Console.WriteLine();
                Console.WriteLine("Instruções para o cliente:");
                Console.WriteLine("  1. Copie o arquivo .lic para a pasta do PRIMAVERA:");
                Console.WriteLine("     C:\\Program Files\\PRIMAVERA\\SG100\\");
                Console.WriteLine("  2. Renomeie o arquivo para: ADAlicePOS.lic");
                Console.WriteLine("  3. Reinicie o PRIMAVERA ERP");
                Console.WriteLine();

                // Mostrar preview da licença
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Preview da licença (primeiros 100 caracteres):");
                Console.WriteLine(license.Substring(0, Math.Min(100, license.Length)) + "...");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erro ao gerar licença: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void ValidateLicenseInteractive()
        {
            Console.WriteLine("═══ Validar Licença ═══");
            Console.WriteLine();

            Console.Write("Caminho do arquivo .lic: ");
            string filePath = Console.ReadLine()?.Trim().Trim('"');

            if (!File.Exists(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Arquivo não encontrado!");
                Console.ResetColor();
                return;
            }

            try
            {
                string licenseContent = File.ReadAllText(filePath);
                var info = DecryptLicense(licenseContent);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Licença válida!");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine($"Hardware ID:  {info.HardwareId}");
                Console.WriteLine($"Empresa:      {info.CompanyName}");
                Console.WriteLine($"Emissão:      {info.IssuedDate:dd/MM/yyyy}");
                Console.WriteLine($"Expiração:    {info.ExpirationDate:dd/MM/yyyy}");
                Console.WriteLine();

                // Verificar status
                if (info.ExpirationDate < DateTime.Now)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠ ATENÇÃO: Licença expirada!");
                    Console.ResetColor();
                }
                else
                {
                    int diasRestantes = (info.ExpirationDate - DateTime.Now).Days;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Dias restantes: {diasRestantes}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erro ao validar licença: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine("USO:");
            Console.WriteLine("  LicenseGenerator.exe                      (modo interativo)");
            Console.WriteLine("  LicenseGenerator.exe [hwid] [dias] [nome] (modo comando)");
            Console.WriteLine();
            Console.WriteLine("EXEMPLO:");
            Console.WriteLine("  LicenseGenerator.exe ABC123...XYZ 365 \"Minha Empresa\"");
        }

        #region License Generation Logic (igual ao LicenseManager)

        static string GetHardwareId()
        {
            string cpuId = GetComponentId("Win32_Processor", "ProcessorId");
            string biosId = GetComponentId("Win32_BIOS", "SerialNumber");
            string boardId = GetComponentId("Win32_BaseBoard", "SerialNumber");

            string combined = $"{cpuId}-{biosId}-{boardId}";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 32);
            }
        }

        static string GetComponentId(string wmiClass, string property)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        object value = obj[property];
                        if (value != null)
                            return value.ToString().Trim();
                    }
                }
            }
            catch { }
            return "UNKNOWN";
        }

        static string GenerateLicense(string hardwareId, DateTime expirationDate, string companyName)
        {
            var info = new LicenseInfo
            {
                HardwareId = hardwareId,
                ExpirationDate = expirationDate,
                CompanyName = companyName,
                IssuedDate = DateTime.Now
            };

            return EncryptLicense(info);
        }

        static string EncryptLicense(LicenseInfo info)
        {
            string plainText = $"{info.HardwareId}|{info.ExpirationDate:yyyy-MM-dd}|{info.CompanyName}|{info.IssuedDate:yyyy-MM-dd}";

            using (Aes aes = Aes.Create())
            {
                aes.Key = DeriveKey(MASTER_KEY);
                aes.GenerateIV();

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        static LicenseInfo DecryptLicense(string encryptedLicense)
        {
            byte[] buffer = Convert.FromBase64String(encryptedLicense);

            using (Aes aes = Aes.Create())
            {
                aes.Key = DeriveKey(MASTER_KEY);

                byte[] iv = new byte[16];
                Array.Copy(buffer, 0, iv, 0, 16);
                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream ms = new MemoryStream(buffer, 16, buffer.Length - 16))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    string plainText = sr.ReadToEnd();
                    string[] parts = plainText.Split('|');

                    if (parts.Length != 4)
                        throw new Exception("Formato de licença inválido.");

                    return new LicenseInfo
                    {
                        HardwareId = parts[0],
                        ExpirationDate = DateTime.Parse(parts[1]),
                        CompanyName = parts[2],
                        IssuedDate = DateTime.Parse(parts[3])
                    };
                }
            }
        }

        static byte[] DeriveKey(string password)
        {
            byte[] salt = Encoding.UTF8.GetBytes("ADAlicePOS_Salt_v10");
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                return pbkdf2.GetBytes(32);
            }
        }

        class LicenseInfo
        {
            public string HardwareId { get; set; }
            public DateTime ExpirationDate { get; set; }
            public string CompanyName { get; set; }
            public DateTime IssuedDate { get; set; }
        }

        #endregion
    }
}
