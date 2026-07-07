using System;
using System.Runtime.InteropServices;

public static class MetodosNativos
{
    public delegate IntPtr ProcedimentoTeclado(int codigoNotificacao, IntPtr parametroW, IntPtr parametroL);
    public delegate IntPtr ProcedimentoMouse(int codigoNotificacao, IntPtr parametroW, IntPtr parametroL);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idGancho, ProcedimentoTeclado procedimento, IntPtr moduloInstancia, uint idThread);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idGancho, ProcedimentoMouse procedimento, IntPtr moduloInstancia, uint idThread);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr identificadorGancho);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr CallNextHookEx(IntPtr identificadorGancho, int codigoNotificacao, IntPtr parametroW, IntPtr parametroL);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint SetThreadExecutionState(uint sinalizadoresEstado);

    public const uint ESTADO_CONTINUO = 0x80000000;
    public const uint SISTEMA_REQUERIDO = 0x00000001;

    [StructLayout(LayoutKind.Sequential)]
    public struct EstruturaTeclado
    {
        public uint codigoTeclaVirtual;
        public uint codigoVarredura;
        public uint sinalizadores;
        public uint tempo;
        public IntPtr informacaoExtra;
    }
}
