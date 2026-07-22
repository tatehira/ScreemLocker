using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

public static class BloqueadorEntrada
{
    private static MetodosNativos.ProcedimentoTeclado _procedimentoTeclado = RetornoGanchoTeclado;
    private static MetodosNativos.ProcedimentoMouse _procedimentoMouse = RetornoGanchoMouse;
    private static IntPtr _identificadorGanchoTeclado = IntPtr.Zero;
    private static IntPtr _identificadorGanchoMouse = IntPtr.Zero;

    // Mensagens de aviso rotativas
    private static readonly string[] _mensagensAviso = new string[]
    {
        "Não foi dessa vez amigao kk",
        "que foi, nao ta conseguindo mexer? 😕",
        "hahaha, coloca o vampeta no seu PC bobão"
    };
    private static int _indiceMensagem = 0;
    private static DateTime _ultimoHorarioNotificacao = DateTime.MinValue;
    private static readonly TimeSpan _intervaloMinimoNotificacao = TimeSpan.FromSeconds(3);

    public static void InstalarGanchos()
    {
        _identificadorGanchoTeclado = DefinirGanchoTeclado(_procedimentoTeclado);
        _identificadorGanchoMouse = DefinirGanchoMouse(_procedimentoMouse);
    }

    public static void DesinstalarGanchos()
    {
        if (_identificadorGanchoTeclado != IntPtr.Zero)
        {
            MetodosNativos.UnhookWindowsHookEx(_identificadorGanchoTeclado);
            _identificadorGanchoTeclado = IntPtr.Zero;
        }
        if (_identificadorGanchoMouse != IntPtr.Zero)
        {
            MetodosNativos.UnhookWindowsHookEx(_identificadorGanchoMouse);
            _identificadorGanchoMouse = IntPtr.Zero;
        }
    }

    private static IntPtr DefinirGanchoTeclado(MetodosNativos.ProcedimentoTeclado proc)
    {
        IntPtr hMod = Marshal.GetHINSTANCE(typeof(BloqueadorEntrada).Module);
        return MetodosNativos.SetWindowsHookEx(13, proc, hMod, 0); // WH_KEYBOARD_LL = 13
    }

    private static IntPtr DefinirGanchoMouse(MetodosNativos.ProcedimentoMouse proc)
    {
        IntPtr hMod = Marshal.GetHINSTANCE(typeof(BloqueadorEntrada).Module);
        return MetodosNativos.SetWindowsHookEx(14, proc, hMod, 0); // WH_MOUSE_LL = 14
    }

    private static IntPtr RetornoGanchoTeclado(int codigoNotificacao, IntPtr parametroW, IntPtr parametroL)
    {
        if (codigoNotificacao >= 0)
        {
            int mensagem = parametroW.ToInt32();
            if (mensagem == 0x0100 || mensagem == 0x0104) // WM_KEYDOWN ou WM_SYSKEYDOWN
            {
                MetodosNativos.EstruturaTeclado estruturaKb = (MetodosNativos.EstruturaTeclado)Marshal.PtrToStructure(parametroL, typeof(MetodosNativos.EstruturaTeclado));
                Keys teclaPressionada = (Keys)estruturaKb.codigoTeclaVirtual;

                if (teclaPressionada == Programa.TeclaAtalho)
                {
                    Programa.AlternarBloqueio();
                    return (IntPtr)1; // Suprime a tecla de atalho
                }

                if (Programa.BloqueioAtivo && Programa.BloquearTeclado)
                {
                    RegistrarInteracao("Teclado");
                    ExibirNotificacaoAviso();
                    return (IntPtr)1;
                }
            }
        }
        return MetodosNativos.CallNextHookEx(_identificadorGanchoTeclado, codigoNotificacao, parametroW, parametroL);
    }

    private static IntPtr RetornoGanchoMouse(int codigoNotificacao, IntPtr parametroW, IntPtr parametroL)
    {
        if (codigoNotificacao >= 0 && Programa.BloqueioAtivo && Programa.BloquearMouse)
        {
            int mensagem = parametroW.ToInt32();
            if (mensagem == 0x0200 || mensagem == 0x0201 || mensagem == 0x0204) // WM_MOUSEMOVE, WM_LBUTTONDOWN, WM_RBUTTONDOWN
            {
                RegistrarInteracao("Mouse");
                ExibirNotificacaoAviso();
            }
            return (IntPtr)1;
        }
        return MetodosNativos.CallNextHookEx(_identificadorGanchoMouse, codigoNotificacao, parametroW, parametroL);
    }
    
    private static void ExibirNotificacaoAviso()
    {
        if (!Programa.MostrarNotificacoes) return;

        DateTime agora = DateTime.Now;
        if (agora - _ultimoHorarioNotificacao >= _intervaloMinimoNotificacao)
        {
            _ultimoHorarioNotificacao = agora;
            string textoMsg = _mensagensAviso[_indiceMensagem];
            _indiceMensagem = (_indiceMensagem + 1) % _mensagensAviso.Length;
            Programa.MostrarBalaoBandeja(textoMsg, ToolTipIcon.Warning);
        }
    }

    private static void RegistrarInteracao(string dispositivo)
    {
        try
        {
            string caminhoLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "interacoes.log");
            using (StreamWriter sw = new StreamWriter(caminhoLog, true))
            {
                sw.WriteLine(string.Format("[{0}] Tentativa de interação detectada no {1}.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), dispositivo));
            }
        }
        catch { }
    }
}
