#region --- License ---
/* Copyright (c) 2006, 2007 Stefanos Apostolopoulos
 * See license.txt for license info
 */
#endregion

#region --- Using directives ---

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using Examples.Shapes;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

#endregion

/// <summary>
/// Generates Array Buffers Objects for Vertices, Normals, TexCoords, Colors, and primitive Indices.
/// The uses DrawElements to draw the primitives from the Array Buffers.
/// Also shows an example of dynamic updating of VBOs.
/// </summary>
public class T08_VBO : GameWindow
{
    #region --- Private Fields ---

    Shape shape = new Cube();
    Vbo vbo = new Vbo();

    int textureID;

	// Flag to indicate it the Array Buffer should dynamically update
    bool dynamicUpdate = true;

    #endregion

    #region --- Constructor ---

    public T08_VBO() : base(800, 600)
    {
		Keyboard.KeyDown += OnKeyDownInternal;
    }

	private void OnKeyDownInternal(object sender, KeyboardKeyEventArgs e) { OnKeyDown(e); }

	/// <summary>
	/// Occurs whenever a keybord key is pressed.
	/// </summary>
	protected virtual void OnKeyDown(KeyboardKeyEventArgs e)
	{
		switch(e.Key)
        {
            // Lighting
            case OpenTK.Input.Key.L:
                if (GL.IsEnabled(EnableCap.Lighting)) GL.Disable(EnableCap.Lighting);
                else GL.Enable(EnableCap.Lighting);
                break;

            // Texture
            case OpenTK.Input.Key.T:
                if (GL.IsEnabled(EnableCap.Texture2D)) GL.Disable(EnableCap.Texture2D);
                else GL.Enable(EnableCap.Texture2D);
                break;

            // Dynamic Update
            case OpenTK.Input.Key.D:
                dynamicUpdate = !dynamicUpdate;
                break;

            // Exit
            case OpenTK.Input.Key.Escape:
                Exit();
                break;
        }
    }

    #endregion

    #region OnLoad override

	protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        GL.ClearColor(0.1f, 0.1f, 0.5f, 0.0f);
        GL.Enable(EnableCap.DepthTest);

        // Vertex Buffers
        vbo = LoadVBO(shape);

        // Lighting
        GL.Enable(EnableCap.Light0);
        GL.Enable(EnableCap.Lighting);

        // Texture
        GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

        GL.GenTextures(1, out textureID);
        GL.BindTexture(TextureTarget.Texture2D, textureID);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

		Bitmap bitmap = new Bitmap("..\\..\\Data\\logo.jpg");
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
        }
        bitmap.UnlockBits(data);

        GL.Enable(EnableCap.Texture2D);
    }

    #endregion

    #region OnResize override

	protected override void OnResize(EventArgs e)
    {
		GL.Viewport(0, 0, Width, Height);

		GL.MatrixMode(MatrixMode.Projection);
		Matrix4 p = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Width / (float)Height, 1.0f, 64.0f);
		GL.LoadMatrix(ref p);

		GL.MatrixMode(MatrixMode.Modelview);
		Matrix4 mv = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
		GL.LoadMatrix(ref mv);
    }

    #endregion

	protected override void OnUpdateFrame(FrameEventArgs e)
    {
        // Dynamic updating of the Vertex Array using different methods
        if (dynamicUpdate)
        {
            if( GL.IsEnabled(EnableCap.Lighting) )
            {
                // Method 1 (BufferData) : Modify local copy and resend data

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.VertexBufferID);

                for (int i = 0; i < shape.Vertices.Length; i++)
                {
                    if (shape.Vertices[i].Y < 0) shape.Vertices[i].Y = -1.0f + (float)Math.Sin(counter*2) / 2.0f;
                    else shape.Vertices[i].Y = 1.0f - (float)Math.Sin(counter * 2) / 2.0f;
                }

                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(shape.Vertices.Length * Vector3.SizeInBytes), shape.Vertices, BufferUsageHint.DynamicDraw);
            }
            else if (GL.IsEnabled(EnableCap.Texture2D))
            {
                // Method 2 (BufferSubData) : Modify PART of local copy and resend data

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.VertexBufferID);

                for (int i = 0; i < shape.Vertices.Length; i++)
                {
                    if (shape.Vertices[i].Y < 0) shape.Vertices[i].Y = -1.0f + (float)Math.Sin(counter * 2) / 2.0f;
                    else shape.Vertices[i].Y = 1.0f - (float)Math.Sin(counter * 2) / 2.0f;
                }

                // Even though we modified all the local data - we are only sending the first half of the changes
                GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(0), (IntPtr)(shape.Vertices.Length * Vector3.SizeInBytes /2), shape.Vertices);
            }
            else
            {
                // Method 3 (MapBuffer) : Bring data local and modify it

                unsafe
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.VertexBufferID);

                    IntPtr intPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadWrite);
                    if (intPtr == IntPtr.Zero) return;

                    Vector3* vertexArray = (Vector3*)intPtr;

                    for (int i = 0; i < shape.Vertices.Length; i++)
                    {
                        if (vertexArray[i].Y < 0) vertexArray[i].Y = -1.0f + (float)Math.Sin(counter * 2) / 2.0f;
                        else vertexArray[i].Y = 1.0f - (float)Math.Sin(counter * 2) / 2.0f;
                    }

                    GL.UnmapBuffer(BufferTarget.ArrayBuffer);
                }
            }
        }
    }

    #region OnRenderFrame

    double counter = 0;
	protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.PushAttrib(AttribMask.LightingBit);
        GL.Disable(EnableCap.Lighting);
        GL.Color3(Color.White);
		/*
        textPrinter.Begin();
        textPrinter.Draw((1.0 / e.Time).ToString("F2"), font);
        GL.Translate(150, 0, 0);
        textPrinter.Draw("(L)ighting  (T)exture  (D)ynamicUpdate", font);
        textPrinter.End();
		*/
		GL.PopAttrib();

        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();
		Matrix4 modelview = Matrix4.LookAt(0.0f, 3.5f, 3.5f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f);
		GL.LoadMatrix(ref modelview);

        counter += e.Time;
        GL.Rotate(counter * 100, 0, 1, 0);

        Draw(vbo);

        SwapBuffers();
    }

    #endregion

    /// <summary>
    /// Generate a VertexBuffer for each of Color, Normal, TextureCoordinate, Vertex, and Indices
    /// </summary>
    Vbo LoadVBO(Shape shape)
    {
        Vbo vbo = new Vbo();

        if (shape.Vertices == null) return vbo;
        if (shape.Indices == null) return vbo;

        int bufferSize;

        // Color Array Buffer
        if (shape.Colors != null)
        {
            // Generate Array Buffer Id
            GL.GenBuffers(1, out vbo.ColorBufferID);

            // Bind current context to Array Buffer ID
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.ColorBufferID);

            // Send data to buffer
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(shape.Colors.Length * sizeof(int)), shape.Colors, BufferUsageHint.StaticDraw);

            // Validate that the buffer is the correct size
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            if (shape.Colors.Length * sizeof(int) != bufferSize)
                throw new ApplicationException("Vertex array not uploaded correctly");

            // Clear the buffer Binding
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        // Normal Array Buffer
        if (shape.Normals != null)
        {
            // Generate Array Buffer Id
            GL.GenBuffers(1, out vbo.NormalBufferID);

            // Bind current context to Array Buffer ID
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.NormalBufferID);

            // Send data to buffer
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(shape.Normals.Length * Vector3.SizeInBytes), shape.Normals, BufferUsageHint.StaticDraw);

            // Validate that the buffer is the correct size
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            if (shape.Normals.Length * Vector3.SizeInBytes != bufferSize)
                throw new ApplicationException("Normal array not uploaded correctly");

            // Clear the buffer Binding
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        // TexCoord Array Buffer
        if (shape.Texcoords != null)
        {
            // Generate Array Buffer Id
            GL.GenBuffers(1, out vbo.TexCoordBufferID);

            // Bind current context to Array Buffer ID
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.TexCoordBufferID);

            // Send data to buffer
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(shape.Texcoords.Length * 8), shape.Texcoords, BufferUsageHint.StaticDraw);

            // Validate that the buffer is the correct size
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            if (shape.Texcoords.Length * 8 != bufferSize)
                throw new ApplicationException("TexCoord array not uploaded correctly");

            // Clear the buffer Binding
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        // Vertex Array Buffer
        {
            // Generate Array Buffer Id
            GL.GenBuffers(1, out vbo.VertexBufferID);

            // Bind current context to Array Buffer ID
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.VertexBufferID);

            // Send data to buffer
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(shape.Vertices.Length * Vector3.SizeInBytes), shape.Vertices, BufferUsageHint.DynamicDraw);

            // Validate that the buffer is the correct size
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            if (shape.Vertices.Length * Vector3.SizeInBytes != bufferSize)
                throw new ApplicationException("Vertex array not uploaded correctly");

            // Clear the buffer Binding
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        // Element Array Buffer
        {
            // Generate Array Buffer Id
            GL.GenBuffers(1, out vbo.ElementBufferID);

            // Bind current context to Array Buffer ID
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo.ElementBufferID);

            // Send data to buffer
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(shape.Indices.Length * sizeof(int)), shape.Indices, BufferUsageHint.StaticDraw);

            // Validate that the buffer is the correct size
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            if (shape.Indices.Length * sizeof(int) != bufferSize)
                throw new ApplicationException("Element array not uploaded correctly");

            // Clear the buffer Binding
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        // Store the number of elements for the DrawElements call
        vbo.NumElements = shape.Indices.Length;

        return vbo;
    }

    void Draw(Vbo vbo)
    {
        // Push current Array Buffer state so we can restore it later
        GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);

        if (vbo.VertexBufferID == 0) return;
        if (vbo.ElementBufferID == 0) return;

        if (GL.IsEnabled(EnableCap.Lighting))
        {
            // Normal Array Buffer
            if (vbo.NormalBufferID != 0)
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.NormalBufferID);

                // Set the Pointer to the current bound array describing how the data ia stored
                GL.NormalPointer(NormalPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);

                // Enable the client state so it will use this array buffer pointer
                GL.EnableClientState(EnableCap.NormalArray);
            }
        }
        else
        {
            // Color Array Buffer (Colors not used when lighting is enabled)
            if (vbo.ColorBufferID != 0)
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.ColorBufferID);

                // Set the Pointer to the current bound array describing how the data ia stored
                GL.ColorPointer(4, ColorPointerType.UnsignedByte, sizeof(int), IntPtr.Zero);

                // Enable the client state so it will use this array buffer pointer
                GL.EnableClientState(EnableCap.ColorArray);
            }
        }

        // Texture Array Buffer
        if (GL.IsEnabled(EnableCap.Texture2D))
        {
            if (vbo.TexCoordBufferID != 0)
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.TexCoordBufferID);

                // Set the Pointer to the current bound array describing how the data ia stored
                GL.TexCoordPointer(2, TexCoordPointerType.Float, 8, IntPtr.Zero);

                // Enable the client state so it will use this array buffer pointer
                GL.EnableClientState(EnableCap.TextureCoordArray);
            }
        }

        // Vertex Array Buffer
        {
            // Bind to the Array Buffer ID
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.VertexBufferID);

            // Set the Pointer to the current bound array describing how the data ia stored
            GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);

            // Enable the client state so it will use this array buffer pointer
            GL.EnableClientState(EnableCap.VertexArray);
        }

        // Element Array Buffer
        {
            // Bind to the Array Buffer ID
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo.ElementBufferID);

            // Draw the elements in the element array buffer
            // Draws up items in the Color, Vertex, TexCoordinate, and Normal Buffers using indices in the ElementArrayBuffer
            GL.DrawElements(BeginMode.Triangles, vbo.NumElements, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Could also call GL.DrawArrays which would ignore the ElementArrayBuffer and just use primitives
            // Of course we would have to reorder our data to be in the correct primitive order
        }

        // Restore the state
        GL.PopClientAttrib();
    }

    #region public static void Main()

    /// <summary>
    /// Entry point of this example.
    /// </summary>
    [STAThread]
    public static void Main()
    {
        using (T08_VBO example = new T08_VBO())
        {
            // Get the title and category of this example using reflection.
            example.Title = String.Format("OpenTK | {0} {1}: {2}", 1, 2, 3);
            example.Run();
        }
    }

    #endregion

    struct Vbo
    {
        public int VertexBufferID;
        public int ColorBufferID;
        public int TexCoordBufferID;
        public int NormalBufferID;
        public int ElementBufferID;
        public int NumElements;
    }
}