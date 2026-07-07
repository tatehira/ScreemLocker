using System;
using System.Drawing;
using System.Windows.Forms;

public class FormularioConfiguracoes : Form
{
    private ComboBox comboTeclaAtalho;
    private CheckBox checkBloquearTeclado;
    private CheckBox checkBloquearMouse;
    private CheckBox checkMostrarNotificacoes;
    private CheckBox checkPrevenirHibernacao;
    private Label rotuloStatus;
    private Button botaoSalvar;
    private Button botaoCancelar;
    private PictureBox caixaImagemLogo;

    public FormularioConfiguracoes()
    {
        InicializarComponentes();
        CarregarConfiguracoesAtuais();
    }

    private void InicializarComponentes()
    {
        this.Text = "KeyShield - Configurações";
        this.Size = new Size(380, 440);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(30, 30, 46); // Catppuccin Base Dark `#1E1E2E`
        this.ForeColor = Color.FromArgb(205, 214, 244); // Light grey text `#CDD6F4`
        this.Font = new Font("Segoe UI", 9.5F);

        try
        {
            Bitmap logo = Programa.CarregarLogo();
            this.Icon = Icon.FromHandle(logo.GetHicon());
        }
        catch { }

        // Painel Superior (Header)
        Panel painelSuperior = new Panel();
        painelSuperior.Dock = DockStyle.Top;
        painelSuperior.Height = 75;
        painelSuperior.BackColor = Color.FromArgb(17, 17, 27); // Darker `#11111B`

        caixaImagemLogo = new PictureBox();
        caixaImagemLogo.Location = new Point(16, 12);
        caixaImagemLogo.Size = new Size(50, 50);
        caixaImagemLogo.SizeMode = PictureBoxSizeMode.Zoom;
        caixaImagemLogo.Image = Programa.CarregarLogo();

        Label rotuloTitulo = new Label();
        rotuloTitulo.Text = "KeyShield";
        rotuloTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        rotuloTitulo.ForeColor = Color.FromArgb(137, 180, 250); // Muted blue `#89B4FA`
        rotuloTitulo.Location = new Point(74, 12);
        rotuloTitulo.Size = new Size(200, 30);

        Label rotuloSubtitulo = new Label();
        rotuloSubtitulo.Text = "Bloqueio seguro de entradas";
        rotuloSubtitulo.Font = new Font("Segoe UI", 8F);
        rotuloSubtitulo.ForeColor = Color.FromArgb(166, 173, 200); // Muted `#A6ADC8`
        rotuloSubtitulo.Location = new Point(76, 42);
        rotuloSubtitulo.Size = new Size(250, 20);

        painelSuperior.Controls.Add(caixaImagemLogo);
        painelSuperior.Controls.Add(rotuloTitulo);
        painelSuperior.Controls.Add(rotuloSubtitulo);

        // Seção da Tecla de Atalho
        Label rotuloTecla = new Label();
        rotuloTecla.Text = "Tecla de ativação (Liga / Desliga):";
        rotuloTecla.Location = new Point(24, 95);
        rotuloTecla.Size = new Size(300, 20);
        rotuloTecla.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

        comboTeclaAtalho = new ComboBox();
        comboTeclaAtalho.DropDownStyle = ComboBoxStyle.DropDownList;
        comboTeclaAtalho.Location = new Point(24, 120);
        comboTeclaAtalho.Size = new Size(316, 25);
        comboTeclaAtalho.BackColor = Color.FromArgb(49, 50, 68); // Soft grey `#313244`
        comboTeclaAtalho.ForeColor = Color.White;
        comboTeclaAtalho.FlatStyle = FlatStyle.Flat;

        foreach (var opt in Programa.TeclasDisponiveis)
        {
            comboTeclaAtalho.Items.Add(opt);
        }

        // Opções de Bloqueio (Checkboxes)
        checkBloquearTeclado = new CheckBox();
        checkBloquearTeclado.Text = "Bloquear Teclado";
        checkBloquearTeclado.FlatStyle = FlatStyle.Flat;
        checkBloquearTeclado.Location = new Point(24, 160);
        checkBloquearTeclado.Size = new Size(300, 25);

        checkBloquearMouse = new CheckBox();
        checkBloquearMouse.Text = "Bloquear Mouse";
        checkBloquearMouse.FlatStyle = FlatStyle.Flat;
        checkBloquearMouse.Location = new Point(24, 188);
        checkBloquearMouse.Size = new Size(300, 25);

        checkMostrarNotificacoes = new CheckBox();
        checkMostrarNotificacoes.Text = "Mostrar avisos e notificações";
        checkMostrarNotificacoes.FlatStyle = FlatStyle.Flat;
        checkMostrarNotificacoes.Location = new Point(24, 216);
        checkMostrarNotificacoes.Size = new Size(300, 25);

        checkPrevenirHibernacao = new CheckBox();
        checkPrevenirHibernacao.Text = "Prevenir hibernação (Mouse Jiggler)";
        checkPrevenirHibernacao.FlatStyle = FlatStyle.Flat;
        checkPrevenirHibernacao.Location = new Point(24, 244);
        checkPrevenirHibernacao.Size = new Size(300, 25);

        // Exibição de Status
        rotuloStatus = new Label();
        rotuloStatus.Location = new Point(24, 280);
        rotuloStatus.Size = new Size(316, 30);
        rotuloStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        rotuloStatus.TextAlign = ContentAlignment.MiddleCenter;
        rotuloStatus.BorderStyle = BorderStyle.None;
        AtualizarRotuloStatus();

        // Botão Salvar
        botaoSalvar = new Button();
        botaoSalvar.Text = "Salvar";
        botaoSalvar.Size = new Size(100, 36);
        botaoSalvar.Location = new Point(130, 335);
        botaoSalvar.FlatStyle = FlatStyle.Flat;
        botaoSalvar.BackColor = Color.FromArgb(137, 180, 250); // Soft Blue
        botaoSalvar.ForeColor = Color.FromArgb(17, 17, 27); // Dark base text
        botaoSalvar.FlatAppearance.BorderSize = 0;
        botaoSalvar.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        botaoSalvar.Cursor = Cursors.Hand;
        botaoSalvar.Click += AoClicarSalvar;
        botaoSalvar.MouseEnter += (s, e) => botaoSalvar.BackColor = Color.FromArgb(180, 190, 254);
        botaoSalvar.MouseLeave += (s, e) => botaoSalvar.BackColor = Color.FromArgb(137, 180, 250);

        // Botão Cancelar/Fechar
        botaoCancelar = new Button();
        botaoCancelar.Text = "Fechar";
        botaoCancelar.Size = new Size(100, 36);
        botaoCancelar.Location = new Point(240, 335);
        botaoCancelar.FlatStyle = FlatStyle.Flat;
        botaoCancelar.BackColor = Color.FromArgb(49, 50, 68);
        botaoCancelar.ForeColor = Color.White;
        botaoCancelar.FlatAppearance.BorderSize = 0;
        botaoCancelar.Cursor = Cursors.Hand;
        botaoCancelar.Click += AoClicarCancelar;
        botaoCancelar.MouseEnter += (s, e) => botaoCancelar.BackColor = Color.FromArgb(69, 71, 90);
        botaoCancelar.MouseLeave += (s, e) => botaoCancelar.BackColor = Color.FromArgb(49, 50, 68);

        this.Controls.Add(painelSuperior);
        this.Controls.Add(rotuloTecla);
        this.Controls.Add(comboTeclaAtalho);
        this.Controls.Add(checkBloquearTeclado);
        this.Controls.Add(checkBloquearMouse);
        this.Controls.Add(checkMostrarNotificacoes);
        this.Controls.Add(checkPrevenirHibernacao);
        this.Controls.Add(rotuloStatus);
        this.Controls.Add(botaoSalvar);
        this.Controls.Add(botaoCancelar);
    }

    private void CarregarConfiguracoesAtuais()
    {
        for (int i = 0; i < comboTeclaAtalho.Items.Count; i++)
        {
            if (((OpcaoTecla)comboTeclaAtalho.Items[i]).ValorTecla == Programa.TeclaAtalho)
            {
                comboTeclaAtalho.SelectedIndex = i;
                break;
            }
        }
        if (comboTeclaAtalho.SelectedIndex == -1 && comboTeclaAtalho.Items.Count > 0)
        {
            comboTeclaAtalho.SelectedIndex = 7; // Atalho padrão F8
        }

        checkBloquearTeclado.Checked = Programa.BloquearTeclado;
        checkBloquearMouse.Checked = Programa.BloquearMouse;
        checkMostrarNotificacoes.Checked = Programa.MostrarNotificacoes;
        checkPrevenirHibernacao.Checked = Programa.PrevenirHibernacao;
    }

    public void AtualizarRotuloStatus()
    {
        if (Programa.BloqueioAtivo)
        {
            rotuloStatus.Text = "STATUS: BLOQUEADO";
            rotuloStatus.BackColor = Color.FromArgb(243, 139, 168); // Red
            rotuloStatus.ForeColor = Color.FromArgb(17, 17, 27);
        }
        else
        {
            rotuloStatus.Text = "STATUS: LIBERADO";
            rotuloStatus.BackColor = Color.FromArgb(166, 227, 161); // Green
            rotuloStatus.ForeColor = Color.FromArgb(17, 17, 27);
        }
    }

    private void AoClicarSalvar(object sender, EventArgs e)
    {
        if (comboTeclaAtalho.SelectedItem != null)
        {
            Programa.TeclaAtalho = ((OpcaoTecla)comboTeclaAtalho.SelectedItem).ValorTecla;
        }
        Programa.BloquearTeclado = checkBloquearTeclado.Checked;
        Programa.BloquearMouse = checkBloquearMouse.Checked;
        Programa.MostrarNotificacoes = checkMostrarNotificacoes.Checked;
        
        Programa.PrevenirHibernacao = checkPrevenirHibernacao.Checked;
        Programa.AtualizarEstadoTimerJiggler();

        Programa.SalvarConfiguracoes();
        Programa.AtualizarMenuBandeja();

        this.Hide();
        if (Programa.MostrarNotificacoes)
        {
            Programa.MostrarBalaoBandeja("Configurações salvas com sucesso!", ToolTipIcon.Info);
        }
    }

    private void AoClicarCancelar(object sender, EventArgs e)
    {
        this.Hide();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            this.Hide();
        }
        else
        {
            base.OnFormClosing(e);
        }
    }
}
