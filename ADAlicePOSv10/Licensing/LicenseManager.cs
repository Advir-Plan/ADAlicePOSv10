using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace ADAlicePOSv10.Licensing
{
    /// <summary>
    /// Gerencia o sistema de licenciamento do ADAlice POS
    /// Valida licenças baseadas em hardware ID e data de expiração
    /// </summary>
    public class LicenseManager
    {
        private const string LICENSE_FILE = "ADAlicePOS.lic";
        private const string MASTER_KEY = "ADAlice_POS_v10_MasterKey_2026_SecureHash"; // Altere isso!

        private static bool _isLicenseValidated = false;
        private static string _cachedHardwareId = null;

        /// <summary>
        /// Obtém o Hardware ID único da máquina
        /// Combina: CPU ID + Motherboard Serial + BIOS Serial
        /// </summary>
        public static string GetHardwareId()
        {
            if (!string.IsNullOrEmpty(_cachedHardwareId))
                return _cachedHardwareId;

            try
            {
                string cpuId = GetComponentId("Win32_Processor", "ProcessorId");
                string biosId = GetComponentId("Win32_BIOS", "SerialNumber");
                string boardId = GetComponentId("Win32_BaseBoard", "SerialNumber");

                // Combina os IDs e gera hash SHA256
                string combined = $"{cpuId}-{biosId}-{boardId}";
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    _cachedHardwareId = BitConverter.ToString(hash).Replace("-", "").Substring(0, 32);
                }

                return _cachedHardwareId;
            }
            catch (Exception ex)
            {
                throw new LicenseException($"Erro ao obter Hardware ID: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém ID de um componente de hardware via WMI
        /// </summary>
        private static string GetComponentId(string wmiClass, string property)
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
            catch
            {
                // Ignora erros de componentes individuais
            }
            return "UNKNOWN";
        }

        /// <summary>
        /// Valida se existe uma licença válida para esta máquina
        /// Deve ser chamado apenas uma vez na inicialização
        /// </summary>
        public static void ValidateLicense()
        {
            string licenseFilePath = GetLicenseFilePath();

            // SEMPRE verifica se o arquivo existe, mesmo que já tenha validado antes
            if (!File.Exists(licenseFilePath))
            {
                throw new LicenseException(
                    "Licença não encontrada!\n\n" +
                    "Entre em contacto com a Equipa Advir para obter sua licença."
                );
            }

            // Se já validou E o arquivo ainda existe, não precisa validar novamente
            if (_isLicenseValidated)
                return;


            try
            {
                string licenseContent = File.ReadAllText(licenseFilePath);
                LicenseInfo info = DecryptLicense(licenseContent);

                // Verifica se o hardware ID corresponde
                if (info.HardwareId != GetHardwareId())
                {
                    throw new LicenseException(
                        "Licença inválida para este computador!\n\n" +
                        "Esta licença não pode ser usada neste computador.\n\n" +
                        "Entre em contacto com o fornecedor para obter uma licença válida."
                    );
                }

                // Verifica se a licença está expirada
                if (info.ExpirationDate < DateTime.Now)
                {
                    throw new LicenseException(
                        $"Licença expirada em {info.ExpirationDate:dd/MM/yyyy}!\n\n" +
                        "Entre em contacto com o fornecedor para renovar sua licença."
                    );
                }

                // Licença válida!
                _isLicenseValidated = true;
            }
            catch (LicenseException)
            {
                throw; // Repassa exceções de licença
            }
            catch (Exception ex)
            {
                throw new LicenseException($"Erro ao validar licença: {ex.Message}");
            }
        }


        /// <summary>
        /// Gera uma licença criptografada (uso interno - para gerador de licenças)
        /// </summary>
        public static string GenerateLicense(string hardwareId, DateTime expirationDate, string companyName)
        {
            LicenseInfo info = new LicenseInfo
            {
                HardwareId = hardwareId,
                ExpirationDate = expirationDate,
                CompanyName = companyName,
                IssuedDate = DateTime.Now
            };

            return EncryptLicense(info);
        }

        /// <summary>
        /// Criptografa informações da licença usando AES-256
        /// </summary>
        private static string EncryptLicense(LicenseInfo info)
        {
            string plainText = $"{info.HardwareId}|{info.ExpirationDate:yyyy-MM-dd}|{info.CompanyName}|{info.IssuedDate:yyyy-MM-dd}";

            using (Aes aes = Aes.Create())
            {
                aes.Key = DeriveKey(MASTER_KEY);
                aes.GenerateIV();

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); // Salva IV no início

                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Descriptografa e valida uma licença
        /// </summary>
        private static LicenseInfo DecryptLicense(string encryptedLicense)
        {
            byte[] buffer = Convert.FromBase64String(encryptedLicense);

            using (Aes aes = Aes.Create())
            {
                aes.Key = DeriveKey(MASTER_KEY);

                // Extrai IV (primeiros 16 bytes)
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
                        throw new LicenseException("Formato de licença inválido.");

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

        /// <summary>
        /// Deriva uma chave de 256 bits da master key usando PBKDF2
        /// </summary>
        private static byte[] DeriveKey(string password)
        {
            byte[] salt = Encoding.UTF8.GetBytes("ADAlicePOS_Salt_v10"); // Salt fixo
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                return pbkdf2.GetBytes(32); // 256 bits
            }
        }

        /// <summary>
        /// Obtém o caminho do arquivo de licença
        /// Armazenado na mesma pasta do executável do PRIMAVERA
        /// </summary>
        private static string GetLicenseFilePath()
        {
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string directory = Path.GetDirectoryName(assemblyPath);
            return Path.Combine(directory, LICENSE_FILE);
        }

        /// <summary>
        /// Reseta o cache de validação (apenas para testes)
        /// </summary>
        public static void ResetValidation()
        {
            _isLicenseValidated = false;
        }

        /// <summary>
        /// Obtém a data de validade da licença atual
        /// </summary>
        /// <returns>Data de expiração da licença ou null se não encontrar</returns>
        public static DateTime? GetLicenseExpirationDate()
        {
            try
            {
                string licenseFilePath = GetLicenseFilePath();

                if (!File.Exists(licenseFilePath))
                    return null;

                string licenseContent = File.ReadAllText(licenseFilePath);
                LicenseInfo info = DecryptLicense(licenseContent);

                return info.ExpirationDate;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Estrutura que contém informações da licença
    /// </summary>
    internal class LicenseInfo
    {
        public string HardwareId { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string CompanyName { get; set; }
        public DateTime IssuedDate { get; set; }
    }

    /// <summary>
    /// Exceção personalizada para erros de licenciamento
    /// </summary>
    public class LicenseException : Exception
    {
        public LicenseException(string message) : base(message) { }
    }
}
