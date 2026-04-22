using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ADAlicePOSv10.Licensing;

namespace ADAlicePOSv10.POS
{
    public class FormPagamentoAlice : Form
    {
        private Panel panelHeader;
        private Label lblTitulo;
        private PictureBox picIcone;
        private Label lblValorPagar;
        private Panel panelPrincipal;
        private Label lblValorRecebido;
        private Label lblValorRecebidoTitulo;
        private Label lblFalta;
        private Label lblFaltaTitulo;
        private Panel panelDivisor;
        private Label lblEstado;
        private Panel panelProgressContainer;
        private ProgressBar progressBar;
        private Button btnCancelar;
        private Panel panelFooter;
        private Label lblDiasRestantes;

        private bool cancelado = false;
        public bool Cancelado => cancelado;

        public FormPagamentoAlice(double valorTotal)
        {
            InitializeComponent(valorTotal);
        }

        private void InitializeComponent(double valorTotal)
        {
            // Configurações do Form
            this.Text = "Pagamento ALICE";
            this.Size = new Size(650, 580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            this.TopMost = true;

            
            // Sombra e bordas arredondadas
            this.Paint += FormPagamentoAlice_Paint;

            // ===== HEADER =====
            panelHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(650, 120),
                BackColor = Color.FromArgb(0, 120, 215)
            };
            panelHeader.Paint += PanelHeader_Paint;

            lblTitulo = new Label
            {
                Text = "💶 PAGAMENTO EM DINHEIRO",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(0, 25),
                Size = new Size(650, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblValorPagar = new Label
            {
                Text = $"{valorTotal:C2}",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(0, 65),
                Size = new Size(650, 45),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Controls.Add(lblValorPagar);

            // ===== PAINEL PRINCIPAL =====
            panelPrincipal = new Panel
            {
                Location = new Point(40, 150),
                Size = new Size(570, 220),
                BackColor = Color.FromArgb(248, 249, 250)
            };
            panelPrincipal.Paint += PanelPrincipal_Paint;

            // Ícone grande
            picIcone = new PictureBox
            {
                Location = new Point(255, 20),
                Size = new Size(60, 60),
                SizeMode = PictureBoxSizeMode.CenterImage
            };
            DesenharIconeRelogio();

            // Valor Recebido - Lado Esquerdo
            lblValorRecebidoTitulo = new Label
            {
                Text = "💰 Recebido",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(60, 100),
                Size = new Size(200, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblValorRecebido = new Label
            {
                Text = "0,00 €",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69),
                Location = new Point(60, 125),
                Size = new Size(200, 40),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Divisor vertical elegante
            panelDivisor = new Panel
            {
                Location = new Point(284, 100),
                Size = new Size(2, 70),
                BackColor = Color.FromArgb(200, 200, 200)
            };

            // Falta - Lado Direito
            lblFaltaTitulo = new Label
            {
                Text = "⏳ Falta",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(310, 100),
                Size = new Size(200, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblFalta = new Label
            {
                Text = $"{valorTotal:C2}",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69),
                Location = new Point(310, 125),
                Size = new Size(200, 40),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Mensagem de estado
            lblEstado = new Label
            {
                Text = "Aguardando inserção de dinheiro...",
                Font = new Font("Segoe UI", 11, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(20, 175),
                Size = new Size(530, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panelPrincipal.Controls.Add(picIcone);
            panelPrincipal.Controls.Add(lblValorRecebidoTitulo);
            panelPrincipal.Controls.Add(lblValorRecebido);
            panelPrincipal.Controls.Add(panelDivisor);
            panelPrincipal.Controls.Add(lblFaltaTitulo);
            panelPrincipal.Controls.Add(lblFalta);
            panelPrincipal.Controls.Add(lblEstado);

            // ===== PROGRESS BAR CONTAINER =====
            panelProgressContainer = new Panel
            {
                Location = new Point(40, 390),
                Size = new Size(570, 40),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            panelProgressContainer.Paint += (s, e) =>
            {
                var rect = panelProgressContainer.ClientRectangle;
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
                }
            };

            progressBar = new ProgressBar
            {
                Location = new Point(10, 10),
                Size = new Size(550, 20),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30
            };

            panelProgressContainer.Controls.Add(progressBar);

            // ===== FOOTER =====
            panelFooter = new Panel
            {
                Location = new Point(0, 510),
                Size = new Size(650, 70),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            btnCancelar = new Button
            {
                Text = "✕  Cancelar e Devolver Dinheiro",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(190, 12),
                Size = new Size(270, 46),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(220, 53, 69),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(220, 53, 69);
            btnCancelar.FlatAppearance.BorderSize = 2;
            btnCancelar.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 245, 245);
            btnCancelar.Click += BtnCancelar_Click;

            // Label de dias restantes
            lblDiasRestantes = new Label
            {
                Text = "-- dias restantes",
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(14, 22),
                Size = new Size(150, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            panelFooter.Controls.Add(lblDiasRestantes);
            panelFooter.Controls.Add(btnCancelar);

            // Carregar informações da licença
            CarregarInfoLicenca();

            // Adicionar todos os controles ao Form
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelPrincipal);
            this.Controls.Add(panelProgressContainer);
            this.Controls.Add(panelFooter);
        }

        /// <summary>
        /// Carrega e exibe informações da licença
        /// </summary>
        private void CarregarInfoLicenca()
        {
            try
            {
                DateTime? dataValidade = LicenseManager.GetLicenseExpirationDate();

                if (dataValidade.HasValue)
                {
                    TimeSpan diasRestantes = dataValidade.Value - DateTime.Now;
                    int dias = (int)Math.Ceiling(diasRestantes.TotalDays);

                    if (dias > 0)
                    {
                        lblDiasRestantes.Text = $"{dias} {(dias == 1 ? "dia restante" : "dias restantes")}";

                        // Cor baseada nos dias restantes
                        if (dias <= 7)
                        {
                            lblDiasRestantes.ForeColor = Color.Red;
                        }
                        else if (dias <= 30)
                        {
                            lblDiasRestantes.ForeColor = Color.Orange;
                        }
                        else
                        {
                            lblDiasRestantes.ForeColor = Color.Green;
                        }
                    }
                    else
                    {
                        lblDiasRestantes.Text = "Licença expirada";
                        lblDiasRestantes.ForeColor = Color.Red;
                    }
                }
                else
                {
                    lblDiasRestantes.Text = "Licença não encontrada";
                    lblDiasRestantes.ForeColor = Color.Red;
                }
            }
            catch
            {
                lblDiasRestantes.Text = "";
                lblDiasRestantes.Visible = false;
            }
        }

        private void FormPagamentoAlice_Paint(object sender, PaintEventArgs e)
        {
            var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (var pen = new Pen(Color.FromArgb(180, 180, 180), 2))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private void PanelHeader_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                panelHeader.ClientRectangle,
                Color.FromArgb(0, 120, 215),
                Color.FromArgb(0, 90, 180),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, panelHeader.ClientRectangle);
            }
        }

        private void PanelPrincipal_Paint(object sender, PaintEventArgs e)
        {
            var rect = new Rectangle(0, 0, panelPrincipal.Width - 1, panelPrincipal.Height - 1);
            using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private void DesenharIconeRelogio()
        {
            Bitmap bmp = new Bitmap(60, 60);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Círculo exterior
                using (Pen pen = new Pen(Color.FromArgb(108, 117, 125), 4))
                {
                    g.DrawEllipse(pen, 5, 5, 50, 50);
                }

                // Ponteiros
                using (Pen pen = new Pen(Color.FromArgb(108, 117, 125), 3))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    // Ponteiro das horas
                    g.DrawLine(pen, 30, 30, 30, 18);
                    // Ponteiro dos minutos
                    g.DrawLine(pen, 30, 30, 42, 30);
                }

                // Centro
                using (Brush brush = new SolidBrush(Color.FromArgb(108, 117, 125)))
                {
                    g.FillEllipse(brush, 27, 27, 6, 6);
                }
            }
            picIcone.Image = bmp;
        }

        private void DesenharIconeSucesso()
        {
            Bitmap bmp = new Bitmap(60, 60);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Círculo verde
                using (Brush brush = new SolidBrush(Color.FromArgb(40, 167, 69)))
                {
                    g.FillEllipse(brush, 0, 0, 60, 60);
                }

                // Check branco
                using (Pen pen = new Pen(Color.White, 6))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLines(pen, new Point[] {
                        new Point(15, 30),
                        new Point(25, 40),
                        new Point(45, 20)
                    });
                }
            }
            picIcone.Image = bmp;
        }

        private void DesenharIconeErro()
        {
            Bitmap bmp = new Bitmap(60, 60);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Círculo vermelho
                using (Brush brush = new SolidBrush(Color.FromArgb(220, 53, 69)))
                {
                    g.FillEllipse(brush, 0, 0, 60, 60);
                }

                // X branco
                using (Pen pen = new Pen(Color.White, 6))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, 20, 20, 40, 40);
                    g.DrawLine(pen, 40, 20, 20, 40);
                }
            }
            picIcone.Image = bmp;
        }

        private void DesenharIconeCancelamento()
        {
            Bitmap bmp = new Bitmap(60, 60);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Círculo laranja
                using (Brush brush = new SolidBrush(Color.FromArgb(255, 153, 0)))
                {
                    g.FillEllipse(brush, 0, 0, 60, 60);
                }

                // Seta de retorno branca
                using (Pen pen = new Pen(Color.White, 5))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    // Curva
                    g.DrawArc(pen, 15, 15, 30, 30, 180, 270);

                    // Seta
                    g.DrawLine(pen, 15, 25, 15, 35);
                    g.DrawLine(pen, 15, 25, 25, 25);
                }
            }
            picIcone.Image = bmp;
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Tem certeza que deseja cancelar o pagamento?\n\n" +
                "💶 O dinheiro inserido será devolvido automaticamente.",
                "⚠ Confirmar Cancelamento",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                btnCancelar.Enabled = false;
                btnCancelar.Text = "⏳ Cancelando...";
                btnCancelar.BackColor = Color.FromArgb(240, 240, 240);

                lblEstado.Text = "Cancelando operação e devolvendo dinheiro...";
                lblEstado.ForeColor = Color.FromArgb(255, 153, 0);
                lblEstado.Font = new Font("Segoe UI", 11, FontStyle.Bold);

                DesenharIconeCancelamento();

                cancelado = true;
            }
        }

        public void AtualizarProgresso(decimal valorRecebido, double valorEsperado, int estado)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AtualizarProgresso(valorRecebido, valorEsperado, estado)));
                return;
            }

            decimal valorRecebidoEuros = valorRecebido / 100;
            decimal falta = (decimal)valorEsperado - valorRecebidoEuros;

            lblValorRecebido.Text = $"{valorRecebidoEuros:C2}";

            if (falta > 0)
            {
                lblFalta.Text = $"{falta:C2}";
                lblFalta.ForeColor = Color.FromArgb(220, 53, 69);
                lblFaltaTitulo.Text = "⏳ Falta";
            }
            else
            {
                lblFalta.Text = "Completo!";
                lblFalta.ForeColor = Color.FromArgb(40, 167, 69);
                lblFaltaTitulo.Text = "✓ Completo";
                lblFaltaTitulo.ForeColor = Color.FromArgb(40, 167, 69);
            }

            // Atualizar estado
            switch (estado)
            {
                case 1:
                    lblEstado.Text = "A iniciar operação no moedeiro...";
                    lblEstado.ForeColor = Color.Gray;
                    break;
                case 2:
                    if (valorRecebidoEuros > 0)
                    {
                        lblEstado.Text = "Continue inserindo dinheiro...";
                        lblEstado.ForeColor = Color.FromArgb(0, 120, 215);
                    }
                    else
                    {
                        lblEstado.Text = "Aguardando inserção de dinheiro...";
                        lblEstado.ForeColor = Color.Gray;
                    }
                    break;
                case 3:
                    lblEstado.Text = "⚙ A cancelar operação, aguarde...";
                    lblEstado.ForeColor = Color.FromArgb(0, 120, 215);
                    break;
                case 7:
                    lblEstado.Text = "Operação em fila de espera...";
                    lblEstado.ForeColor = Color.FromArgb(255, 153, 0);
                    break;
                case 8:
                    lblEstado.Text = "Operação não encontrada na Alice.";
                    lblEstado.ForeColor = Color.FromArgb(220, 53, 69);
                    break;
            }
        }

        public void MostrarSucesso(decimal valorRecebido, decimal troco)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MostrarSucesso(valorRecebido, troco)));
                return;
            }

            decimal valorRecebidoEuros = valorRecebido / 100;
            decimal trocoEuros = troco / 100;

            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 100;
            progressBar.ForeColor = Color.FromArgb(40, 167, 69);

            DesenharIconeSucesso();

            lblEstado.Text = "✓ Pagamento concluído com sucesso!";
            lblEstado.ForeColor = Color.FromArgb(40, 167, 69);
            lblEstado.Font = new Font("Segoe UI", 13, FontStyle.Bold);

            lblFaltaTitulo.Text = "💶 Troco";
            lblFaltaTitulo.ForeColor = Color.FromArgb(40, 167, 69);
            lblFalta.Text = trocoEuros > 0 ? $"{trocoEuros:C2}" : "Sem troco";
            lblFalta.ForeColor = Color.FromArgb(40, 167, 69);

            // Mudar cor do header para verde
            panelHeader.Paint -= PanelHeader_Paint;
            panelHeader.Paint += (s, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    panelHeader.ClientRectangle,
                    Color.FromArgb(40, 167, 69),
                    Color.FromArgb(30, 140, 55),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, panelHeader.ClientRectangle);
                }
            };
            panelHeader.Invalidate();

            btnCancelar.Visible = false;

            // Fechar automaticamente após 3 segundos
            Timer timer = new Timer { Interval = 3000 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            timer.Start();
        }

        public void MostrarErro(string mensagem)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MostrarErro(mensagem)));
                return;
            }

            string msg = string.IsNullOrWhiteSpace(mensagem)
                ? "Erro no processamento do pagamento."
                : mensagem.Trim();

            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;

            bool isCancelamento =
                msg.IndexOf("cancelada", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("devolvido", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("tempo limite", StringComparison.OrdinalIgnoreCase) >= 0;

            if (isCancelamento)
            {
                DesenharIconeCancelamento();

                if (msg.IndexOf("tempo limite", StringComparison.OrdinalIgnoreCase) >= 0)
                    msg = "Tempo limite excedido.\nDinheiro devolvido.";

                lblEstado.Text = msg;
                lblEstado.ForeColor = Color.FromArgb(255, 153, 0);
                lblEstado.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                lblEstado.Size = new Size(530, 60);
                lblEstado.AutoSize = false;

                panelHeader.Paint -= PanelHeader_Paint;
                panelHeader.Paint += HeaderLaranja_Paint;
                panelHeader.Invalidate();
            }
            else
            {
                DesenharIconeErro();

                lblEstado.Text = msg;
                lblEstado.ForeColor = Color.FromArgb(220, 53, 69);
                lblEstado.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                lblEstado.Location = new Point(20, 165);
                lblEstado.Size = new Size(530, 90);
                lblEstado.AutoSize = false;

                panelHeader.Paint -= PanelHeader_Paint;
                panelHeader.Paint += HeaderVermelho_Paint;
                panelHeader.Invalidate();
            }

            btnCancelar.Text = "✕  Fechar";
            btnCancelar.BackColor = Color.FromArgb(108, 117, 125);
            btnCancelar.ForeColor = Color.White;
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(108, 117, 125);
            btnCancelar.Enabled = true;
            btnCancelar.Click -= BtnCancelar_Click;
            btnCancelar.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
        }

        private void HeaderLaranja_Paint(object sender, PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(
                panelHeader.ClientRectangle,
                Color.FromArgb(255, 153, 0),
                Color.FromArgb(230, 130, 0),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, panelHeader.ClientRectangle);
            }
        }

        private void HeaderVermelho_Paint(object sender, PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(
                panelHeader.ClientRectangle,
                Color.FromArgb(220, 53, 69),
                Color.FromArgb(190, 40, 55),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, panelHeader.ClientRectangle);
            }
        }


        public void MostrarAviso(string mensagem)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MostrarAviso(mensagem)));
                return;
            }

            // Mostrar aviso temporário no label de estado sem bloquear
            // Salvar cor e texto atual
            var corAnterior = lblEstado.ForeColor;
            var textoAnterior = lblEstado.Text;

            // Mostrar aviso em laranja
            lblEstado.ForeColor = Color.FromArgb(255, 153, 0);
            lblEstado.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblEstado.Text = $"⚠ {mensagem}";

            // Após 3 segundos, restaurar texto anterior
            Timer timer = new Timer { Interval = 3000 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                if (!lblEstado.Text.StartsWith("✓") && !lblEstado.Text.Contains("Erro"))
                {
                    lblEstado.ForeColor = corAnterior;
                    lblEstado.Font = new Font("Segoe UI", 11, FontStyle.Italic);
                    lblEstado.Text = textoAnterior;
                }
            };
            timer.Start();

            System.Diagnostics.Debug.WriteLine($">>> Aviso mostrado: {mensagem}");
        }
    }
}
