using System;
using System.Windows.Forms;
using Primavera.Extensibility.BusinessEntities.ExtensibilityPattern;

namespace ADAlicePOSv10.Utils
{
    /// <summary>
    /// Classe auxiliar para abrir editores e formulários da aplicação
    /// </summary>
    public static class EditorHelper
    {
        /// <summary>
        /// Abre o editor de definições da Alice
        /// </summary>
        /// <param name="extensibility">Objeto de extensibilidade do Primavera</param>
        /// <returns>True se as configurações foram guardadas, False se foi cancelado</returns>
        public static bool AbrirEditorDefinicoesAlice(PriExtensibility extensibility)
        {
            try
            {
                var editor = new DefenicoesAlice(extensibility);
                var resultado = editor.ShowDialog();
                return resultado == DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao abrir editor de definições:\n{ex.Message}",
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
        }
    }
}
