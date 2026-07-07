using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;

public static class Programa
{
    public static bool BloqueioAtivo = false;
    public static Keys TeclaAtalho = Keys.F8;
    public static bool BloquearTeclado = true;
    public static bool BloquearMouse = true;
    public static bool MostrarNotificacoes = true;
    public static bool PrevenirHibernacao = true;
    private static NotifyIcon _iconeBandeja;
    private static ContextMenu _menuContexto;
    private static MenuItem _menuItemConfiguracoes;
    private static MenuItem _menuItemBloquear;
    private static MenuItem _menuItemSair;
    private static FormularioConfiguracoes _formularioConfiguracoes;
    private static System.Windows.Forms.Timer _timerJiggler;

    private static Mutex _mutexInstancia = new Mutex(true, "{KEYSHIELD-MUTEX-UNIQUE-ID-7821}");

    public static readonly OpcaoTecla[] TeclasDisponiveis = new OpcaoTecla[]
    {
        new OpcaoTecla("F1", Keys.F1),
        new OpcaoTecla("F2", Keys.F2),
        new OpcaoTecla("F3", Keys.F3),
        new OpcaoTecla("F4", Keys.F4),
        new OpcaoTecla("F5", Keys.F5),
        new OpcaoTecla("F6", Keys.F6),
        new OpcaoTecla("F7", Keys.F7),
        new OpcaoTecla("F8", Keys.F8),
        new OpcaoTecla("F9", Keys.F9),
        new OpcaoTecla("F10", Keys.F10),
        new OpcaoTecla("F11", Keys.F11),
        new OpcaoTecla("F12", Keys.F12),
        new OpcaoTecla("Scroll Lock", Keys.Scroll),
        new OpcaoTecla("Pause / Break", Keys.Pause),
        new OpcaoTecla("Caps Lock", Keys.CapsLock),
        new OpcaoTecla("Num Lock", Keys.NumLock),
        new OpcaoTecla("Insert (Ins)", Keys.Insert),
        new OpcaoTecla("Home", Keys.Home),
        new OpcaoTecla("End", Keys.End),
        new OpcaoTecla("Page Up", Keys.PageUp),
        new OpcaoTecla("Page Down", Keys.PageDown)
    };

    [STAThread]
    public static void Main()
    {
        if (!_mutexInstancia.WaitOne(TimeSpan.Zero, true))
        {
            MessageBox.Show("O KeyShield já está rodando em segundo plano na barra de tarefas (próximo ao relógio).", "KeyShield", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        CarregarConfiguracoes();

        InicializarBandeja();

        BloqueadorEntrada.InstalarGanchos();

        _timerJiggler = new System.Windows.Forms.Timer();
        _timerJiggler.Interval = 60000; // 1 minuto
        _timerJiggler.Tick += AoTickTimerJiggler;
        if (PrevenirHibernacao)
        {
            _timerJiggler.Start();
        }

        if (MostrarNotificacoes)
        {
            _iconeBandeja.ShowBalloonTip(3000, "KeyShield Iniciado", "Rodando em segundo plano. Dê duplo clique no ícone para abrir as configurações.", ToolTipIcon.Info);
        }

        Application.Run();

        if (_timerJiggler != null)
        {
            _timerJiggler.Stop();
            _timerJiggler.Dispose();
        }

        _mutexInstancia.ReleaseMutex();
    }

    private static void InicializarBandeja()
    {
        _iconeBandeja = new NotifyIcon();
        
        Bitmap logo = CarregarLogo();
        try
        {
            _iconeBandeja.Icon = Icon.FromHandle(logo.GetHicon());
        }
        catch
        {
            _iconeBandeja.Icon = SystemIcons.Shield;
        }

        _menuContexto = new ContextMenu();
        _menuItemConfiguracoes = new MenuItem("Configurações", AoClicarConfiguracoes);
        _menuItemConfiguracoes.DefaultItem = true;
        _menuItemBloquear = new MenuItem("Bloquear", AoClicarBloquear);
        _menuItemSair = new MenuItem("Sair", AoClicarSair);

        _menuContexto.MenuItems.Add(_menuItemConfiguracoes);
        _menuContexto.MenuItems.Add(_menuItemBloquear);
        _menuContexto.MenuItems.Add("-");
        _menuContexto.MenuItems.Add(_menuItemSair);

        _iconeBandeja.ContextMenu = _menuContexto;
        _iconeBandeja.Text = "KeyShield - Proteção Ativa";
        _iconeBandeja.Visible = true;
        _iconeBandeja.DoubleClick += AoClicarConfiguracoes;
    }

    public static void MostrarBalaoBandeja(string msg, ToolTipIcon icone)
    {
        if (_iconeBandeja != null)
        {
            _iconeBandeja.ShowBalloonTip(1500, "KeyShield", msg, icone);
        }
    }

    public static void AtualizarEstadoTimerJiggler()
    {
        if (_timerJiggler != null)
        {
            if (PrevenirHibernacao)
            {
                if (!_timerJiggler.Enabled) _timerJiggler.Start();
            }
            else
            {
                if (_timerJiggler.Enabled) _timerJiggler.Stop();
            }
        }
    }

    public static void AlternarBloqueio()
    {
        BloqueioAtivo = !BloqueioAtivo;
        AtualizarMenuBandeja();

        if (BloqueioAtivo)
        {
            Console.Beep(800, 150);
            if (MostrarNotificacoes)
            {
                _iconeBandeja.ShowBalloonTip(1500, "KeyShield", "Bloqueio de tela ativado.", ToolTipIcon.Warning);
            }
        }
        else
        {
            Console.Beep(1200, 150);
            if (MostrarNotificacoes)
            {
                _iconeBandeja.ShowBalloonTip(1500, "KeyShield", "Acesso liberado.", ToolTipIcon.Info);
            }
        }

        // Atualiza formulário aberto se ativo
        if (_formularioConfiguracoes != null && !_formularioConfiguracoes.IsDisposed && _formularioConfiguracoes.Visible)
        {
            _formularioConfiguracoes.AtualizarRotuloStatus();
        }
    }

    public static void AtualizarMenuBandeja()
    {
        _menuItemBloquear.Text = BloqueioAtivo ? "Desbloquear" : "Bloquear";
        _iconeBandeja.Text = "KeyShield - " + (BloqueioAtivo ? "Bloqueado" : "Proteção Ativa");
    }

    private static void AoTickTimerJiggler(object sender, EventArgs e)
    {
        if (!PrevenirHibernacao) return;

        try
        {
            // Impede a hibernação do Windows enviando sinalização do sistema
            MetodosNativos.SetThreadExecutionState(MetodosNativos.ESTADO_CONTINUO | MetodosNativos.SISTEMA_REQUERIDO);

            // Move cursor 1 pixel para simular uso físico (apenas se não estiver bloqueado para evitar triggers desnecessários)
            if (!BloqueioAtivo)
            {
                Point posicaoAtual = Cursor.Position;
                Cursor.Position = new Point(posicaoAtual.X + 1, posicaoAtual.Y);
                Thread.Sleep(10);
                Cursor.Position = posicaoAtual;
            }
        }
        catch { }
    }

    private static void AoClicarConfiguracoes(object sender, EventArgs e)
    {
        if (_formularioConfiguracoes == null || _formularioConfiguracoes.IsDisposed)
        {
            _formularioConfiguracoes = new FormularioConfiguracoes();
        }
        _formularioConfiguracoes.Show();
        _formularioConfiguracoes.Activate();
    }

    private static void AoClicarBloquear(object sender, EventArgs e)
    {
        AlternarBloqueio();
    }

    private static void AoClicarSair(object sender, EventArgs e)
    {
        // Desinstala os ganchos do sistema
        BloqueadorEntrada.DesinstalarGanchos();

        if (_iconeBandeja != null)
        {
            _iconeBandeja.Visible = false;
            _iconeBandeja.Dispose();
        }

        Application.Exit();
    }

    public static Bitmap CarregarLogo()
    {
        try
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("KeyShield.logo.png"))
            {
                if (stream != null)
                {
                    return new Bitmap(stream);
                }
            }
        }
        catch { }

        // Desenho vetorial de reserva para a logo
        Bitmap fallback = new Bitmap(128, 128);
        using (Graphics g = Graphics.FromImage(fallback))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(30, 30, 46));
            
            Point[] pontos = {
                new Point(64, 20),
                new Point(104, 30),
                new Point(104, 70),
                new Point(64, 110),
                new Point(24, 70),
                new Point(24, 30)
            };
            g.FillPolygon(new SolidBrush(Color.FromArgb(137, 180, 250)), pontos);

            g.DrawArc(new Pen(Color.FromArgb(17, 17, 27), 6), 49, 45, 30, 30, 180, 180);
            g.FillRectangle(new SolidBrush(Color.FromArgb(17, 17, 27)), 44, 60, 40, 25);
        }
        return fallback;
    }

    private static void CarregarConfiguracoes()
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keyshield.config");
        if (File.Exists(path))
        {
            try
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    if (string.IsNullOrEmpty(line) || !line.Contains("=")) continue;
                    string[] parts = line.Split(new char[] { '=' }, 2);
                    string key = parts[0].Trim();
                    string val = parts[1].Trim();
                    if (key == "Hotkey")
                    {
                        Keys temp;
                        if (Enum.TryParse<Keys>(val, out temp))
                        {
                            TeclaAtalho = temp;
                        }
                    }
                    else if (key == "BlockKeyboard")
                    {
                        bool.TryParse(val, out BloquearTeclado);
                    }
                    else if (key == "BlockMouse")
                    {
                        bool.TryParse(val, out BloquearMouse);
                    }
                    else if (key == "ShowNotifications")
                    {
                        bool.TryParse(val, out MostrarNotificacoes);
                    }
                    else if (key == "PreventSleep")
                    {
                        bool.TryParse(val, out PrevenirHibernacao);
                    }
                }
            }
            catch { }
        }
    }

    public static void SalvarConfiguracoes()
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keyshield.config");
        try
        {
            using (StreamWriter sw = new StreamWriter(path, false))
            {
                sw.WriteLine("Hotkey=" + TeclaAtalho.ToString());
                sw.WriteLine("BlockKeyboard=" + BloquearTeclado.ToString());
                sw.WriteLine("BlockMouse=" + BloquearMouse.ToString());
                sw.WriteLine("ShowNotifications=" + MostrarNotificacoes.ToString());
                sw.WriteLine("PreventSleep=" + PrevenirHibernacao.ToString());
            }
        }
        catch { }
    }
}
