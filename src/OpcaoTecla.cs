using System.Windows.Forms;

public class OpcaoTecla
{
    public string NomeExibicao { get; set; }
    public Keys ValorTecla { get; set; }

    public OpcaoTecla(string nome, Keys valor)
    {
        NomeExibicao = nome;
        ValorTecla = valor;
    }

    public override string ToString()
    {
        return NomeExibicao;
    }
}
