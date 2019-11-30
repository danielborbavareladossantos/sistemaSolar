/**
Fonte: https://github.com/mono/opentk/blob/master/Source/Examples/OpenGL/1.x/Textures.cs
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using CG_Biblioteca;

namespace trabalho
{
  class Mundo : GameWindow
  {
    //FIXME: precisei instalar $ brew install mono-libgdiplus
    Bitmap bitmap = new Bitmap("logoGCG.png");
    private Transformacao4D matrizTerra = new Transformacao4D();
    private Transformacao4D matrizLua = new Transformacao4D();
    private Vector3 eye, at;
    private float far;


    int texture;

    public Mundo(int width, int height) : base(width, height) { }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      GL.ClearColor(Color.Black);
      GL.Enable(EnableCap.DepthTest);
      GL.Enable(EnableCap.CullFace);

      //TODO: o que faz está linha abaixo?
      GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
      GL.GenTextures(1, out texture);
      GL.BindTexture(TextureTarget.Texture2D, texture);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

      BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
          ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
          OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

      bitmap.UnlockBits(data);

            // ___ parâmetros da câmera sintética
      eye = new Vector3(50, 50, 50);
      at = new Vector3(0, 0, 0);
      far = 100.0f;

      // Matriz terra
      Transformacao4D matrizTranslate1 = new Transformacao4D();
      matrizTranslate1.AtribuirTranslacao(10, 0, 0);
      matrizTerra = matrizTranslate1.MultiplicarMatriz(matrizTerra);
      Transformacao4D matrizScale = new Transformacao4D();
      matrizScale.AtribuirEscala(1.5, 1.5, 1.5);
      matrizTerra = matrizScale.MultiplicarMatriz(matrizTerra);
    
      // Matriz lua
      Transformacao4D matrizTranslate2 = new Transformacao4D();
      matrizTranslate2.AtribuirTranslacao(6, 0, 0);
      matrizLua = matrizTranslate2.MultiplicarMatriz(matrizLua);
      matrizTranslate2.AtribuirRotacaoY(Transformacao4D.DEG_TO_RAD * 20);
      matrizLua = matrizTranslate2.MultiplicarMatriz(matrizLua);

    }

    protected override void OnUnload(EventArgs e)
    {
      GL.DeleteTextures(1, ref texture);
    }

    protected override void OnResize(EventArgs e)
    {
      base.OnResize(e);
      GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
      Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, far);
      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadMatrix(ref projection);
    }
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
      Matrix4 modelview = Matrix4.LookAt(eye, at, new Vector3(0,1,0));
      GL.MatrixMode(MatrixMode.Modelview);
      GL.LoadMatrix(ref modelview);

      SRU3D();

      // desenha Sol
      DesenhaCubo();

      // desnha terra
      GL.PushMatrix();                                    // N3-Exe14: grafo de cena
        GL.MultMatrix(matrizTerra.ObterDados());
        DesenhaCubo(); // terra
        GL.PushMatrix();                                    // N3-Exe14: grafo de cena
          GL.MultMatrix(matrizLua.ObterDados());
          DesenhaCubo(); // lua
        GL.PopMatrix();                                     // N3-Exe14: grafo de cena

      GL.PopMatrix();                                     // N3-Exe14: grafo de cena

      SwapBuffers();
    }

    protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
    {
      if (e.Key == Key.Escape)
        this.Exit();
      else
        if (e.Key == Key.F)
        GL.CullFace(CullFaceMode.Front);
      if (e.Key == Key.B)
        GL.CullFace(CullFaceMode.Back);
      if (e.Key == Key.A)
        //FIXME: aqui deveria aplicar a textura no lado de fora e dentro, mas não aparece nada
        GL.CullFace(CullFaceMode.FrontAndBack);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
    }

    private void DesenhaCubo()
    {
      // GL.Enable(EnableCap.Texture2D);
      // GL.BindTexture(TextureTarget.Texture2D, texture);

      GL.Color3(Color.White);

      drawSphere(3, 99, 99);

      // GL.Disable(EnableCap.Texture2D);
    }

    void drawSphere(double r, int lats, int longs) {
      int i, j;
      for(i = 0; i <= lats; i++) {
          double lat0 = Math.PI * (-0.5 + (double) (i - 1) / lats);
          double z0  = Math.Sin(lat0);
          double zr0 =  Math.Cos(lat0);

          double lat1 = Math.PI * (-0.5 + (double) i / lats);
          double z1 = Math.Sin(lat1);
          double zr1 = Math.Cos(lat1);

          GL.Begin(PrimitiveType.QuadStrip);
          for(j = 0; j <= longs; j++) {
              double lng = 2 * Math.PI * (double) (j - 1) / longs;
              double x = Math.Cos(lng);
              double y = Math.Sin(lng);

              GL.Normal3(x * zr0, y * zr0, z0);
              GL.Vertex3(r * x * zr0, r * y * zr0, r * z0);
              GL.Normal3(x * zr1, y * zr1, z1);
              GL.Vertex3(r * x * zr1, r * y * zr1, r * z1);
          }
          GL.End();
      }
    }

    private void SRU3D()
    {
      GL.LineWidth(3);
      GL.Begin(PrimitiveType.Lines);
      GL.Color3(Color.Red);
      GL.Vertex3(0, 0, 0); GL.Vertex3(200, 0, 0);
      GL.Color3(Color.Green);
      GL.Vertex3(0, 0, 0); GL.Vertex3(0, 200, 0);
      GL.Color3(Color.Blue);
      GL.Vertex3(0, 0, 0); GL.Vertex3(0, 0, 200);
      GL.End();
    }

  }

  class Program
  {
    static void Main(string[] args)
    {
      Mundo window = new Mundo(600, 600);
      window.Run(1.0 / 60.0);
    }
  }

}