using AForge.Video;
using AForge.Video.DirectShow;

public partial class FormCamera : Form
{
    private FilterInfoCollection dispositivosDeVideo;
    private VideoCaptureDevice fonteDeVideo;

    public FormCamera()
    {
        InitializeComponent();
        CarregarDispositivos();
    }

    private void CarregarDispositivos()
    {
        dispositivosDeVideo = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        foreach (FilterInfo dispositivo in dispositivosDeVideo)
        {
            comboBoxCameras.Items.Add(dispositivo.Name);
        }
        
        if (comboBoxCameras.Items.Count > 0)
            comboBoxCameras.SelectedIndex = 0;
    }

    private void btnIniciar_Click(object sender, EventArgs e)
    {
        if (comboBoxCameras.Items.Count == 0) return;

        EncerrarWebcam();

        string nomeDoDispositivo = dispositivosDeVideo[comboBoxCameras.SelectedIndex].MonikerString;
        fonteDeVideo = new VideoCaptureDevice(nomeDoDispositivo);
        
        fonteDeVideo.NewFrame += new NewFrameEventHandler(fonteDeVideo_NewFrame);
        fonteDeVideo.Start();
    }

    private void fonteDeVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
    {
        Bitmap imagem = (Bitmap)eventArgs.Frame.Clone();
        
        pictureBoxCamera.Image = imagem;
    }

    private void btnTirarFoto_Click(object sender, EventArgs e)
    {
        if (pictureBoxCamera.Image != null)
        {
            pictureBoxCamera.Image.Save("foto_capturada.png", System.Drawing.Imaging.ImageFormat.Png);
            MessageBox.Show("Foto salva com sucesso!");
        }
    }

    private void EncerrarWebcam()
    {
        if (fonteDeVideo != null && fonteDeVideo.IsRunning)
        {
            fonteDeVideo.SignalToStop();
            fonteDeVideo.WaitForStop();
            fonteDeVideo = null;
        }
    }

    private void FormCamera_FormClosing(object sender, FormClosingEventArgs e)
    {
        EncerrarWebcam();
    }
}
