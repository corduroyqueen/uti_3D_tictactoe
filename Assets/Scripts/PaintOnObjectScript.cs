using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PaintOnObjectScript : MonoBehaviour
{
    /// This script + paintshader in the shaders folder is adapted from code I found online
    /// it paints the walls
    
    [Range(16, 8182)]
    public int textureSize = 64;
    
    private readonly Color c_color = new Color(0, 0, 0, 0);

    private Material m_material;
    private Texture2D m_texture;
    private bool m_isEnabled = false;

    public Texture2D[] projectileSplashTextures;

    private List<float[,]> m_projSplashTextures;
    private int m_projSplashTexturesCount;

    void Start()//this code grabs the shader and creates a transparent texture to draw the paint textures on
    {
        Renderer renderer = GetComponent<Renderer>();
        if (null == renderer)
            return;

        foreach (Material material in renderer.materials)
        {
            if (material.shader.name.Contains("Custom/"))
            {
                m_material = material;
                break;
            }
        }

        if (null != m_material)
        {
            m_texture = new Texture2D(textureSize, textureSize);
            for (int x = 0; x < textureSize; ++x)
                for (int y = 0; y < textureSize; ++y)
                    m_texture.SetPixel(x, y, c_color);
            m_texture.Apply();

            m_material.SetTexture("_DrawingTex", m_texture);
            m_isEnabled = true;
        }

        m_projSplashTexturesCount = projectileSplashTextures.Length;
        m_projSplashTextures = new List<float[,]>(m_projSplashTexturesCount);
        for (int i = 0; i < m_projSplashTexturesCount; ++i)
        {
            Texture2D texture = projectileSplashTextures[i];
            int textureWidth = texture.width;
            int textureHeight = texture.height;
            Color[] currTexture = texture.GetPixels();
            float[,] textureAlphas = new float[textureWidth, textureHeight];
            int counter = 0;
            for (int x = 0; x < textureWidth; ++x)
            {
                for (int y = 0; y < textureHeight; ++y)
                {
                    textureAlphas[x, y] = currTexture[counter].a;
                    counter++;
                }
            }
            m_projSplashTextures.Add(textureAlphas);
        }
    }
    /// this function paints the blank texture with a random splash texture
    private void PaintOn(Vector2 textureCoord, float[,] splashTexture, Color targetColor)
    {
        if (m_isEnabled)
        {
            int reqnx = splashTexture.GetLength(0);
            int reqny = splashTexture.GetLength(1);
            int reqX = (int)(textureCoord.x * textureSize) - (reqnx / 2);
            int reqY = (int)(textureCoord.y * textureSize) - (reqny / 2);
            int right = m_texture.width - 1;
            int bottom = m_texture.height - 1;
            int x = IntMax(reqX, 0);
            int y = IntMax(reqY, 0);
            int nx = IntMin(x + reqnx, right) - x;
            int ny = IntMin(y + reqny, bottom) - y;
            Color[] pixels = m_texture.GetPixels(x, y, nx, ny);
            int counter = 0;
            for (int i = 0; i < nx; ++i)
            {
                for (int j = 0; j < ny; ++j)
                {
                    float currAlpha = splashTexture[i, j];
                    if (currAlpha == 1)
                        pixels[counter] = targetColor;
                    else
                    {
                        Color currColor = pixels[counter];
                        Color newColor = Color.Lerp(currColor, targetColor, currAlpha);
                        newColor.a = pixels[counter].a + currAlpha;
                        pixels[counter] = newColor;
                    }
                    counter++;
                }
            }
            m_texture.SetPixels(x, y, nx, ny, pixels);
            m_texture.Apply();
        }
    }
    //int max/min functions
    private int IntMax(int a, int b)
    {
        return a > b ? a : b;
    }

    private int IntMin(int a, int b)
    {
        return a < b ? a : b;
    }

    public float[,] GetRandomProjectileSplash()
    {
        return m_projSplashTextures[IntMin(Random.Range(0, m_projSplashTexturesCount),2)];
    }

    /// This function detects when the ball hits the paintable walls
    /// then it throws a raycast from the ball into the wall to get the textureCoord which determines where to draw the paint texture
    /// then it draws it at that location
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GameObject ball = collision.gameObject;
            PaintballShoot ballScript = ball.GetComponent<PaintballShoot>();
            RaycastHit hit;
            if (Physics.Raycast(ball.transform.position- ballScript.velocity*0.5f, ballScript.velocity, out hit, 20f))
            {
                Renderer rend = hit.transform.GetComponent<Renderer>();
                MeshCollider meshCollider = hit.collider as MeshCollider;

                if (rend == null || rend.sharedMaterial == null ||
                    rend.sharedMaterial.mainTexture == null || meshCollider == null)
                {
                    return;
                }
                PaintOn(hit.textureCoord, GetRandomProjectileSplash(), ballScript.playerColors[ballScript.playerNum]);
            }
            
        }
    }
}
